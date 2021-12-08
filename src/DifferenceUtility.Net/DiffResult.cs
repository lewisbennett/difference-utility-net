using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.CollectionUpdateCallbacks;
using DifferenceUtility.Net.Helper;

namespace DifferenceUtility.Net
{
    /// <summary>
    /// <para>This class hold the information about the result of a <see cref="DiffUtil.CalculateDiff{T,T}" /> call.</para>
    /// <para>You can consume updates in a DiffResult via <see cref="DispatchUpdatesTo(ObservableCollection{TOld})" />.</para>
    /// </summary>
    public class DiffResult<TOld, TNew>
    {
        #region Fields
        private readonly IList<int> _path;
        private readonly IDiffCallback<TOld, TNew> _diffCallback;
        private readonly TNew[] _newArray;
        private readonly TOld[] _oldArray;
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Dispatches the update events to the given collection.
        /// </summary>
        /// <param name="observableCollection">A collection which is displaying the old collection, and will start displaying the new collection.</param>
        public void DispatchUpdatesTo([NotNull] ObservableCollection<TOld> observableCollection)
        {
            if (IsEmpty())
                return;
            
            if (observableCollection is null)
                throw new ArgumentNullException(nameof(observableCollection));
            
            DispatchUpdatesTo(new ObservableCollectionUpdateCallback<TOld, TNew>(_diffCallback, observableCollection, _oldArray, _newArray));
        }
        
        /// <summary>
        /// <para>Dispatches update operations to the given callback.</para>
        /// <para>These updates are atomic such that the first update call affects every update call that comes after it.</para>
        /// </summary>
        /// <param name="updateCallback">The callback to receive the update operations.</param>
        public void DispatchUpdatesTo([NotNull] ICollectionUpdateCallback updateCallback)
        {
            if (IsEmpty())
                return;
            
            if (updateCallback is null)
                throw new ArgumentNullException(nameof(updateCallback));
            
            if (updateCallback is not BatchingCollectionUpdateCallback batchingCallback)
                batchingCallback = new BatchingCollectionUpdateCallback(updateCallback);

            var postponedUpdates = new HashSet<(int X, int Y)>();

            var currentX = -1;
            var currentY = -1;

            var offsets = new Dictionary<int, int>();

            foreach (var payload in _path)
            {
                // Diagonal movement.
                if ((payload & DiffOperation.NoOperation) != 0)
                {
                    // Diagonals don't contain any coordinates, so just increment normally.
                    currentX++;
                    currentY++;

                    if ((payload & DiffOperation.Update) != 0)
                    {
                        var finalOffset = 0;
                        
                        // Apply any offsets.
                        foreach (var (xPosition, offset) in offsets)
                        {
                            if (currentX + finalOffset >= xPosition)
                                finalOffset += offset;
                        }
                        
                        batchingCallback.OnChanged(currentX + finalOffset, currentY, 1);
                    }

                    continue;
                }

                (int X, int Y) postponedUpdate;

                // Vertical movement.
                if ((payload & DiffOperation.Insert) != 0)
                {
                    currentY++;

                    // Regular insertion if no move flag.
                    if ((payload & DiffOperation.Move) == 0)
                    {
                        var mutableCurrentY = currentY;

                        // Subtract the number of postponed updates where the Y value is less than the current Y.
                        // This is to correct for any moves that haven't happened yet that would normally affect
                        // the current Y coordinate, had they been processed normally.
                        foreach (var update in postponedUpdates)
                        {
                            if (update.Y < currentY)
                                mutableCurrentY--;
                        }
                        
                        batchingCallback.OnInserted(mutableCurrentY, currentY);
                        // batchingCallback.OnInserted(mutableCurrentY, 1);

                        if (offsets.TryGetValue(mutableCurrentY, out var existingOffset))
                            offsets[mutableCurrentY] = existingOffset + 1;
                        
                        else
                            offsets[mutableCurrentY] = 1;
                        
                        continue;
                    }

                    // X coordinate encoded in payload.
                    var x = payload >> DiffOperation.Offset;

                    // Try to find an existing postponed update with the provided coordinates.
                    if (!postponedUpdates.TryGetValue((x, currentY), out postponedUpdate))
                    {
                        postponedUpdates.Add((x, currentY));
                        continue;
                    }
                }
                // Horizontal movement.
                else if ((payload & DiffOperation.Remove) != 0)
                {
                    currentX++;

                    // Regular removal if no move flag.
                    if ((payload & DiffOperation.Move) == 0)
                    {
                        var removeAt = payload >> DiffOperation.Offset;

                        // Apply any offsets.
                        foreach (var (xPosition, offset) in offsets)
                        {
                            if (removeAt >= xPosition)
                                removeAt += offset;
                        }
                        
                        batchingCallback.OnRemoved(removeAt, 1);
                        
                        if (offsets.TryGetValue(removeAt, out var existingOffset))
                            offsets[removeAt] = existingOffset - 1;
                        
                        else
                            offsets[removeAt] = -1;

                        continue;
                    }

                    // Y coordinate encoded in payload.
                    var y = payload >> DiffOperation.Offset;

                    // Try to find an existing postponed update with the provided coordinates.
                    if (!postponedUpdates.TryGetValue((currentX, y), out postponedUpdate))
                    {
                        postponedUpdates.Add((currentX, y));
                        continue;
                    }
                }
                else
                    continue;

                postponedUpdates.Remove(postponedUpdate);

                // Apply any offsets.
                foreach (var (xPosition, offset) in offsets)
                {
                    if (postponedUpdate.X >= xPosition)
                        postponedUpdate.X += offset;
                }

                var yOffset = 0;
                
                foreach (var (updateX, updateY) in postponedUpdates)
                {
                    // Future moves (postponed updates) may affect our current target index (Y coordinate of the postponed update we're handling).
                    // Such operations will have a start (X coordinate) and finish (Y coordinate) position on either side of our current target index.
                    // Operations that exist purely on one side of our current target index will not affect the current target index when they are handled.
                    // We must apply an offset to counter the effect of any future operations that will affect our current target index in order for items
                    // to be moved correctly now, and result in the correct position later. Refer to the below example:
                    
                    // Source:      B A D F H L O C Z
                    // Destination: A Z H B D F C

                    // During the path for the above data, we reach a point where the collection looks like this: A B D F H L O C Z
                    // As you can see in the destination, 'H' comes before the sequence 'B D F', which has already been established due to prior moves.
                    // In the calculated path, 'H' is instructed to move to position 2, which is its final position in the destination collection, and
                    // also where it would already be if move detection was disabled. Since we're using move detection however, this position requires
                    // offsetting in order to be correct by the end of the sequence.
                    
                    // We can see that 'Z', which is currently located at the end of the collection, comes before 'H' in the final destination. This is
                    // a prime example of the offset described above and calculated below. The operation to move 'Z' is currently postponed and therefore,
                    // 'Z' is not located at position 1 yet, which means that 'H' would be in the wrong position if we were to move it to the exact value
                    // encoded in its payload. The postponed update for 'Z' has coordinates X8, Y1. The target index for 'H' (2) fits between these values,
                    // so now we know it will be affected when the postponed update for 'Z' is handled. Since the target index for 'Z' (1) is less than 'H',
                    // we know the position of 'H' will increase, so we negatively offset the target index for 'H' in order for everything to line up later.
                    
                    // This is confusing, I know.

                    var mutableUpdateX = updateX;
                    
                    foreach (var (xPosition, offset) in offsets)
                    {
                        if (mutableUpdateX >= xPosition)
                            mutableUpdateX += offset;
                    }

                    if (mutableUpdateX <= postponedUpdate.Y)
                    {
                        yOffset++;
                    }
                    else if (updateY < postponedUpdate.Y)
                    {
                        yOffset--;
                    }

                    // if (updateX <= postponedUpdate.X && updateY >= postponedUpdate.Y)
                    //     yOffset++;
                    //
                    // else if (updateX >= postponedUpdate.X && updateY <= postponedUpdate.Y)
                    //     yOffset--;
                }

                postponedUpdate.Y += yOffset;
                
                if ((payload & DiffOperation.Update) != 0)
                    batchingCallback.OnChanged(postponedUpdate.X, postponedUpdate.Y, 1);
                
                batchingCallback.OnMoved(postponedUpdate.X, postponedUpdate.Y);
            }

            batchingCallback.DispatchLastEvent();
        }
        #endregion
        
        #region Constructors
        internal DiffResult(IDiffCallback<TOld, TNew> diffCallback, TOld[] oldArray, TNew[] newArray, IList<int> path)
        {
            _path = path;
            _diffCallback = diffCallback;
            _newArray = newArray;
            _oldArray = oldArray;
        }
        #endregion
        
        #region Private Methods
        private bool IsEmpty()
        {
            return _path is null || _path.Count == 0 || _diffCallback is null || _oldArray is null || _newArray is null || _oldArray.Length == 0 && _newArray.Length == 0;
        }
        #endregion
        
        #region Static Fields
        private static DiffResult<TOld, TNew> _emptyDiffResult;
        #endregion
        
        #region Internal Static Methods
        /// <summary>
        /// Gets an empty <see cref="DiffResult{TOld,TNew}" /> that takes zero action when applied.
        /// </summary>
        internal static DiffResult<TOld, TNew> Empty()
        {
            return _emptyDiffResult ??= new DiffResult<TOld, TNew>(null, null, null, null);
        }

        /// <summary>
        /// Gets a configured <see cref="DiffResult{TOld,TNew}" /> that ignores diagonals.
        /// </summary>
        internal static DiffResult<TOld, TNew> NoDiagonals(IDiffCallback<TOld, TNew> diffCallback, TOld[] oldArray, TNew[] newArray)
        {
            var path = new int[oldArray.Length + newArray.Length];
            
            // Add X/remove operations first.
            for (var x = 0; x < oldArray.Length; x++)
                path[x] = (x << DiffOperation.Offset) | DiffOperation.Remove;
            
            // Then add Y/insert operations.
            for (var y = 0; y < newArray.Length; y++)
                path[y + oldArray.Length] = (y << DiffOperation.Offset) | DiffOperation.Insert;

            return new DiffResult<TOld, TNew>(diffCallback, oldArray, newArray, path);
        }
        #endregion
    }
}
