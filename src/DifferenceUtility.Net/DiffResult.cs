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
        where TNew : class
        where TOld : class
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

                // Tracks the number of postponed updates with a Y coordinate less than the current operation's Y value.
                var postponedUpdateCount = 0;
                
                foreach (var update in postponedUpdates)
                {
                    if (postponedUpdate.Y > update.Y)
                        postponedUpdateCount++;
                }

                postponedUpdate.Y -= postponedUpdateCount;
                
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
    }
}
