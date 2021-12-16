using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.CollectionUpdateCallbacks;
using DifferenceUtility.Net.Helper;

namespace DifferenceUtility.Net
{
    /// <summary>
    /// <para>This class holds the information about the result of a <see cref="DiffUtil.CalculateDiff{T,T}" /> call.</para>
    /// <para>You can consume updates in a DiffResult via <see cref="DispatchUpdatesTo(ObservableCollection{TOld})" />.</para>
    /// </summary>
    public class DiffResult<TOld, TNew>
    {
        #region Fields
        private readonly int[] _path;
        private readonly IDiffCallback<TOld, TNew> _diffCallback;
        private readonly int _moveCount;
        private readonly TNew[] _newArray;
        private SortedDictionary<int, (int From, int Offset)> _offsets;
        private readonly TOld[] _oldArray;
        private List<PostponedOperation> _postponedOperations;
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

            if (IsEmpty())
                return;

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
            
            if (IsEmpty())
                return;

            if (updateCallback is not BatchingCollectionUpdateCallback batchingCallback)
                batchingCallback = new BatchingCollectionUpdateCallback(updateCallback);
            
            _offsets = new SortedDictionary<int, (int From, int Offset)>();
            
            if (_moveCount > 0)
                _postponedOperations = new List<PostponedOperation>(_moveCount);
            
            var currentX = -1;
            var currentY = -1;
            
            for (var operationId = 0; operationId < _path.Length; operationId++)
            {
                var operation = _path[operationId];

                // The only way an operation can be zero is if it's a diagonal without the update flag.
                if (operation == 0)
                {
                    // Diagonal operations don't contain any coordinates, so just increment normally.
                    currentX++;
                    currentY++;
                    
                    continue;
                }

                PostponedOperation postponedOperation;
                
                // Vertical movement.
                if ((operation & DiffOperation.Insert) != 0)
                {
                    currentY++;

                    // Regular insertion if no move flag.
                    if ((operation & DiffOperation.Move) == 0)
                    {
                        var offsetY = OffsetY(currentY, currentY);
                        
                        batchingCallback.OnInserted(offsetY, currentY);

                        CreateXOffset(offsetY, 1, operationId);

                        continue;
                    }

                    // X coordinate encoded in payload.
                    var x = operation >> DiffOperation.Offset;
                    
                    // Try to find an existing postponed operation with the provided coordinates.
                    if (!TryFindPostponedOperation(_postponedOperations, x, currentY, out postponedOperation))
                    {
                        _postponedOperations.Add(new PostponedOperation
                        {
                            OperationID = operationId,
                            X = x,
                            Y = currentY
                        });
                        
                        continue;
                    }
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
                    if (!TryFindPostponedOperation(_postponedOperations, currentX, y, out postponedOperation))
                    {
                        _postponedOperations.Add(new PostponedOperation
                        {
                            OperationID = operationId,
                            X = currentX,
                            Y = y
                        });
                        
                        continue;
                    }
                }
                // If the operation isn't an insert or remove, but has the update flag, treat it as a diagonal.
                else if ((operation & DiffOperation.Update) != 0)
                {
                    // Diagonal operations don't contain any coordinates, so just increment normally.
                    currentX++;
                    currentY++;
                    
                    batchingCallback.OnChanged(OffsetX(currentX), currentY, 1);
                    
                    continue;
                }
                else
                    continue;

                _postponedOperations.Remove(postponedOperation);

                // Apply offsets.
                var offsetPostponedX = OffsetX(postponedOperation.X);
                var offsetPostponedY = OffsetY(offsetPostponedX, postponedOperation.Y);
                
                if ((operation & DiffOperation.Update) != 0)
                    batchingCallback.OnChanged(offsetPostponedX, postponedOperation.Y, 1);

                batchingCallback.OnMoved(offsetPostponedX, offsetPostponedY);
                
                // Create the offsets as a result of the move operation.
                if (offsetPostponedX > offsetPostponedY)
                {
                    CreateXOffset(offsetPostponedY, 1, postponedOperation.OperationID);
                    CreateXOffset(offsetPostponedX + 1, -1, operationId);
                }
                else if (offsetPostponedX < offsetPostponedY)
                {
                    CreateXOffset(offsetPostponedX, -1, postponedOperation.OperationID);
                    CreateXOffset(offsetPostponedY, 1, operationId);
                }
            }

            batchingCallback.DispatchLastEvent();
        }
        #endregion
        
        #region Constructors
        internal DiffResult(IDiffCallback<TOld, TNew> diffCallback, TOld[] oldArray, TNew[] newArray, int[] path, int moveCount = 0)
        {
            _path = path;
            _diffCallback = diffCallback;
            _moveCount = moveCount;
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
                
                // The provided offset should be applied to offsets that are positioned after the provided 'from' position.
                if (from < queryFrom)
                    _offsets[queryOperationId] = (queryFrom + offset, queryOffset);
            }
            
            // Finally, create the offset.
            _offsets[operationId] = (from, offset);
        }
        
        private bool IsEmpty()
        {
            return _path is null || _path.Length == 0 || _diffCallback is null || _oldArray is null || _newArray is null || _oldArray.Length == 0 && _newArray.Length == 0;
        }
        
        private int OffsetX(int x)
        {
            var offsets = _offsets.ToDictionary(o => o.Key, o => o.Value);

            var xOffset = 0;
            
            while (true)
            {
                var newXOffset = 0;

                foreach (var offset in _offsets)
                {
                    if (!offsets.ContainsKey(offset.Key))
                        continue;

                    var tempXOffset = newXOffset + xOffset;
                    
                    if (x + tempXOffset >= offset.Value.From)
                        newXOffset += offset.Value.Offset;
                    
                    else
                        continue;

                    offsets.Remove(offset.Key);
                    
                    break;
                }
                
                if (newXOffset == 0)
                    break;

                xOffset += newXOffset;
            }
            
            // foreach (var (_, (from, offset)) in _offsets)
            // {
            //     if (x >= from)
            //         x += offset;
            // }

            return x + xOffset;
        }

        private int OffsetY(int offsetX, int y)
        {
            // Postponed operations may be null if there are no moves in the provided path.
            if (_postponedOperations is null)
                return y;
            
            var yOffset = 0;

            var postponedOperations = _postponedOperations.ToList();

            while (true)
            {
                var newYOffset = 0;
                
                foreach (var postponedOperation in _postponedOperations)
                {
                    if (!TryFindPostponedOperation(postponedOperations, postponedOperation.X, postponedOperation.Y, out _))
                        continue;
                    
                    var offsetOperationX = OffsetX(postponedOperation.X);

                    var tempYOffset = yOffset + newYOffset;
                    
                    // Insertions only, not moves.
                    if (postponedOperation.Y < y && offsetOperationX >= y + tempYOffset)
                        newYOffset--;
                    
                    else if (postponedOperation.Y > y && offsetOperationX <= y + tempYOffset)
                        newYOffset++;
                    
                    else
                        continue;
                    
                    
                    // The postponed operation's X value would increase if we were to insert at the current adjusted Y,
                    // and the postponed operation's X value is greater than or equal to the adjusted Y.
                    // if (offsetOperationX < offsetX && offsetOperationX >= y + tempYOffset)
                    //     offsetOperationX++;
                    //
                    // else if (offsetOperationX > offsetX && offsetOperationX <= y + tempYOffset)
                    //     offsetOperationX--;
                    //
                    // // Comparing the operation's Y value to the provided, unmodified Y value tells us where the items are meant
                    // // to be in relation to each other at the end of the sequence. We use this information to determine whether
                    // // the postponed operation will cause the item at the current Y position to move later on.
                    //
                    // // If the item represented by the postponed operation is positioned before the item represented by Y in the destination collection,
                    // // check if the postponed item is currently positioned after the current item to determine whether the operation will provide a positive
                    // // offset when it is eventually processed. If it will, decrement the Y offset to counter this.
                    // if (operationCoordinates.Y < y && offsetOperationX > y + tempYOffset)
                    //     newYOffset--;
                    //
                    // else if (operationCoordinates.Y > y && offsetOperationX < y + tempYOffset)
                    //     newYOffset++;
                    //
                    // else
                    //     continue;

                    postponedOperations.Remove(postponedOperation);
                    
                    break;
                }
                
                if (newYOffset == 0)
                    break;

                yOffset += newYOffset;
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
        
        #region Private Static Methods
        private static bool TryFindPostponedOperation(List<PostponedOperation> postponedOperations, int x, int y, out PostponedOperation postponedOperation)
        {
            using (var enumerator = postponedOperations.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.X != x || enumerator.Current.Y != y)
                        continue;

                    postponedOperation = enumerator.Current;
                    return true;
                }
            }

            postponedOperation = new PostponedOperation
            {
                OperationID = -1,
                X = -1,
                Y = -1
            };
            
            return false;
        }
        #endregion
    }
}
