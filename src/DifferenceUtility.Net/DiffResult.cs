using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
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
        private SortedDictionary<int, (int From, int Offset)> _offsets;
        private readonly TOld[] _oldArray;
        private Dictionary<(int X, int Y), int> _postponedOperations;
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
            
            _offsets = new SortedDictionary<int, (int From, int Offset)>();
            _postponedOperations = new Dictionary<(int X, int Y), int>();
            
            var currentX = -1;
            var currentY = -1;
            
            for (var operationId = 0; operationId < _path.Count; operationId++)
            {
                var operation = _path[operationId];

                // Diagonal movement.
                if ((operation & DiffOperation.NoOperation) != 0)
                {
                    // Diagonals don't contain any coordinates, so just increment normally.
                    currentX++;
                    currentY++;
                    
                    if ((operation & DiffOperation.Update) != 0)
                        batchingCallback.OnChanged(OffsetX(currentX), currentY, 1);

                    continue;
                }
                
                int postponedOperationId;
                int postponedX;
                int postponedY;
                
                // Vertical movement.
                if ((operation & DiffOperation.Insert) != 0)
                {
                    currentY++;

                    // Regular insertion if no move flag.
                    if ((operation & DiffOperation.Move) == 0)
                    {
                        var offsetY = OffsetY(currentY);
                        
                        batchingCallback.OnInserted(offsetY, currentY);

                        CreateXOffset(offsetY, 1, operationId);

                        continue;
                    }

                    // X coordinate encoded in payload.
                    var x = operation >> DiffOperation.Offset;

                    // Try to find an existing postponed operation with the provided coordinates.
                    if (!_postponedOperations.TryGetValue((x, currentY), out postponedOperationId))
                    {
                        _postponedOperations[(x, currentY)] = operationId;
                        continue;
                    }
                    
                    postponedX = x;
                    postponedY = currentY;
                }
                // Horizontal movement.
                else if ((operation & DiffOperation.Remove) != 0)
                {
                    currentX++;

                    // Regular removal if no move flag.
                    if ((operation & DiffOperation.Move) == 0)
                    {
                        var offsetX = OffsetX(currentX);

                        batchingCallback.OnRemoved(offsetX, 1);

                        CreateXOffset(offsetX, -1, operationId);

                        continue;
                    }

                    // Y coordinate encoded in payload.
                    var y = operation >> DiffOperation.Offset;
                    
                    // Try to find an existing postponed operation with the provided coordinates.
                    if (!_postponedOperations.TryGetValue((currentX, y), out postponedOperationId))
                    {
                        _postponedOperations[(currentX, y)] = operationId;
                        continue;
                    }

                    postponedX = currentX;
                    postponedY = y;
                }
                else
                    continue;

                _postponedOperations.Remove((postponedX, postponedY));

                // Apply offsets.
                var offsetPostponedX = OffsetX(postponedX);
                var offsetPostponedY = OffsetY(postponedY);
                
                if ((operation & DiffOperation.Update) != 0)
                    batchingCallback.OnChanged(offsetPostponedX, postponedY, 1);

                batchingCallback.OnMoved(offsetPostponedX, offsetPostponedY);

                // Create the offsets as a result of the move operation.
                if (offsetPostponedX > offsetPostponedY)
                {
                    CreateXOffset(offsetPostponedY + 1, 1, postponedOperationId);
                    CreateXOffset(offsetPostponedX + 1, -1, operationId);
                }
                else if (offsetPostponedX < offsetPostponedY)
                {
                    CreateXOffset(offsetPostponedX, -1, postponedOperationId);
                    CreateXOffset(offsetPostponedY, 1, operationId);
                }
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
        private void CreateXOffset(int from, int offset, int operationId)
        {
            for (var i = 0; i < _offsets.Count; i++)
            {
                var (queryOperationId, (queryFrom, queryOffset)) = _offsets.ElementAt(i);

                // Offset the offset's 'from' position if it's affected by the new offset.
                if (queryFrom > from)
                    _offsets[queryOperationId] = (queryFrom + offset, queryOffset);
            }
            
            // Finally, create the offset.
            _offsets[operationId] = (from, offset);
        }
        
        private bool IsEmpty()
        {
            return _path is null || _path.Count == 0 || _diffCallback is null || _oldArray is null || _newArray is null || _oldArray.Length == 0 && _newArray.Length == 0;
        }
        
        private int OffsetX(int x)
        {
            foreach (var (_, (from, offset)) in _offsets)
            {
                if (x >= from)
                    x += offset;
            }

            return x;
        }

        private int OffsetY(int y)
        {
            var yOffset = 0;

            foreach (var ((operationX, operationY), _) in _postponedOperations)
            {
                var offsetOperationX = OffsetX(operationX);

                // The postponed operation's X value would increase if we were to insert at the current adjusted Y,
                // and the postponed operation's X value is greater than or equal to the adjusted Y.
                if (offsetOperationX >= y + yOffset)
                    offsetOperationX++;
                
                // Decrement the offset for operations who's Y value is less than Y and are currently located after Y, relative to the adjusted position.
                if (operationY < y && offsetOperationX > y + yOffset)
                    yOffset--;
                
                // Increment the offset for operations who's Y value is greater than Y and are currently located before Y, relative to the adjusted position.
                else if (operationY > y && offsetOperationX < y + yOffset)
                    yOffset++;
            }
            
            return y + yOffset;
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
