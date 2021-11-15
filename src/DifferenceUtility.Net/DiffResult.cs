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
    /// <para>This class hold the information about the result of a <see cref="DiffUtil.CalculateDiff{T,T}" /> call.</para>
    /// <para>You can consume updates in a DiffResult via <see cref="DispatchUpdatesTo(ObservableCollection{TOld})" />.</para>
    /// </summary>
    public class DiffResult<TOld, TNew>
    {
        #region Fields
        private readonly bool _detectMoves;
        private readonly IList<Diagonal> _diagonals;
        private readonly IDiffCallback<TOld, TNew> _diffCallback;
        private readonly TNew[] _newArray;
        private readonly int[] _newItemStatuses, _oldItemStatuses;
        private readonly TOld[] _oldArray;
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Given a position in the new collection, returns the position in the old collection, or <see cref="NoPosition" /> if not present.
        /// </summary>
        /// <param name="newItemPosition">The position of the item in the new collection.</param>
        /// <returns>The position of the item in the old collection, or <see cref="NoPosition" /> if not present.</returns>
        public int ConvertNewPositionToOld(int newItemPosition)
        {
            if (newItemPosition < 0 || newItemPosition >= _newArray.Length)
                throw new IndexOutOfRangeException($"Index {newItemPosition} out of bounds. New collection size: {_newArray.Length}");

            var status = _newItemStatuses[newItemPosition];

            return (status & FlagMask) == 0 ? NoPosition : status >> FlagOffset;
        }
        
        /// <summary>
        /// Given a position in the old collection, returns the position in the new collection, or <see cref="NoPosition" /> if it was removed.
        /// </summary>
        /// <param name="oldItemPosition">The position of the item in the old collection.</param>
        /// <returns>The position of the item in the new collection, or <see cref="NoPosition" /> if not present.</returns>
        /// <seealso cref="ConvertNewPositionToOld" />
        public int ConvertOldPositionToNew(int oldItemPosition)
        {
            if (oldItemPosition < 0 || oldItemPosition >= _oldArray.Length)
                throw new IndexOutOfRangeException($"Index {oldItemPosition} out of bounds. Old collection size: {_oldArray.Length}");

            var status = _oldItemStatuses[oldItemPosition];

            return (status & FlagMask) == 0 ? -1 : status >> FlagOffset;
        }
        
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
        /// These updates are atomic such that the first update call affects every update call that comes after it.
        /// </summary>
        /// <param name="updateCallback">The callback to receive the update operations.</param>
        public void DispatchUpdatesTo([NotNull] ICollectionUpdateCallback updateCallback)
        {
            if (updateCallback is null)
                throw new ArgumentNullException(nameof(updateCallback));
            
            if (updateCallback is not BatchingCollectionUpdateCallback batchingCallback)
                batchingCallback = new BatchingCollectionUpdateCallback(updateCallback);
            
            // Track up to date current list size for moves.
            // When a move is found, we record its position from the end of the collection (which is likely to change since we iterate in reverse).
            // Later when we find the match of that move, we dispatch the update.
            var currentCollectionSize = _oldArray.Length;
            
            // List of postponed moves.
            var postponedUpdates = new List<PostponedUpdate>();
            
            // positionX and positionY are exclusive.
            var positionX = _oldArray.Length;
            var positionY = _newArray.Length;
            
            // Iterate from end of the list to the beginning.
            // This just makes offsets easier since changes in the earlier indices has an effect on the later indices.
            for (var diagonalIndex = _diagonals.Count - 1; diagonalIndex >= 0; diagonalIndex--)
            {
                var diagonal = _diagonals[diagonalIndex];

                var endX = diagonal.GetEndX();
                var endY = diagonal.GetEndY();
                
                // Dispatch removals and additions until we reach to that diagonal first removal
                // then add so that it can go into its place and we don't need to offset values.
                while (positionX > endX)
                {
                    positionX--;
                    
                    // Removal.
                    var status = _oldItemStatuses[positionX];

                    switch (status & FlagMoved)
                    {
                        // Simple removal
                        case 0:
                            
                            batchingCallback.OnRemoved(positionX, 1);
                            currentCollectionSize--;

                            break;
                        
                        default:
                            
                            var newPosition = status >> FlagOffset;
                        
                            // Get postponed addition.
                            if (GetPostponedUpdate(postponedUpdates, newPosition, false) is { } postponedUpdate)
                            {
                                // This is an addition that was postponed. Now dispatch it.
                                var updatedNewPosition = currentCollectionSize - postponedUpdate.CurrentPosition;
                            
                                batchingCallback.OnMoved(positionX, updatedNewPosition - 1);
                            
                                if ((status & FlagMovedChanged) != 0)
                                    batchingCallback.OnChanged(updatedNewPosition - 1, newPosition, 1);
                            }
                            // First time we are seeing this, we'll see a matching addition.
                            else
                                postponedUpdates.Add(new PostponedUpdate(positionX, currentCollectionSize - positionX - 1, true));
                            
                            break;
                    }
                }

                while (positionY > endY)
                {
                    positionY--;
                    
                    // Addition.
                    var status = _newItemStatuses[positionY];

                    switch (status & FlagMoved)
                    {
                        // Simple addition.
                        case 0:
                            
                            batchingCallback.OnInserted(positionX, 1);
                            currentCollectionSize++;
                            
                            break;
                        
                        default:
                            
                            // This is a move, not an addition.
                            // See if this is postponed.
                            var oldPosition = status >> FlagOffset;
                            
                            // Get postponed removal.
                            // Postpone it until we see the removal.
                            if (GetPostponedUpdate(postponedUpdates, oldPosition, true) is { } postponedUpdate)
                            {
                                // oldPositionFromEnd = foundCollectionSize = posX
                                // We can find posX if we swap the collection sizes.
                                // posX = collectionSize - oldPositionFromEnd
                                var updatedOldPosition = currentCollectionSize - postponedUpdate.CurrentPosition - 1;
                                
                                batchingCallback.OnMoved(updatedOldPosition, positionX);
                                
                                if ((status & FlagMovedChanged) != 0)
                                    batchingCallback.OnChanged(positionX, positionY, 1);
                            }
                            else
                                postponedUpdates.Add(new PostponedUpdate(positionY, currentCollectionSize - positionX, false));
                            
                            break;
                    }
                }
                
                // Now dispatch updates for the diagonal.
                positionX = diagonal.X;
                positionY = diagonal.Y;

                for (var i = 0; i < diagonal.Size; i++)
                {
                    // Dispatch changes.
                    if ((_oldItemStatuses[positionX] & FlagMask) == FlagChanged)
                        batchingCallback.OnChanged(positionX, positionY, 1);

                    positionX++;
                    positionY++;
                }
                
                // Snap back for the next diagonal.
                positionX = diagonal.X;
                positionY = diagonal.Y;
            }
            
            batchingCallback.DispatchLastEvent();
        }
        #endregion
        
        #region Constructors
        internal DiffResult(IDiffCallback<TOld, TNew> diffCallback, TOld[] oldArray, TNew[] newArray, IList<Diagonal> diagonals, int[] oldItemStatuses, int[] newItemStatuses, bool detectMoves)
        {
            _detectMoves = detectMoves;
            _diagonals = diagonals;
            _diffCallback = diffCallback;
            _newArray = newArray;
            _newItemStatuses = newItemStatuses;
            _oldArray = oldArray;
            _oldItemStatuses = oldItemStatuses;
            
            Array.Fill(_oldItemStatuses, 0);
            Array.Fill(_newItemStatuses, 0);
            
            AddEdgeDiagonals();
            FindMatchingItems();
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Add edge diagonals so that we can iterate as long as there are diagonals without lots of null checks around.
        /// </summary>
        private void AddEdgeDiagonals()
        {
            // See if we should add 1 to the 0, 0.
            if (!_diagonals.Any())
                _diagonals.Add(new Diagonal(0, 0, 0));
            
            else if (_diagonals.First() is not { X: 0, Y: 0 })
                _diagonals.Insert(0, new Diagonal(0, 0, 0));
            
            // Always add one last.
            _diagonals.Add(new Diagonal(_oldArray.Length, _newArray.Length, 0));
        }

        /// <summary>
        /// Search the whole list to find the addition for the given removal of position <paramref name="positionX" />.
        /// </summary>
        /// <param name="positionX">The position in the old collection.</param>
        private void FindMatchingAddition(int positionX)
        {
            var positionY = 0;
            
            foreach (var diagonal in _diagonals)
            {
                while (positionY < diagonal.Y)
                {
                    // Found some additions, evaluate.
                    if (_newItemStatuses[positionY] == 0)
                    {
                        // Not evaluated yet.
                        var oldItem = _oldArray[positionX];
                        var newItem = _newArray[positionY];
                        
                        if (_diffCallback.AreItemsTheSame(oldItem, newItem))
                        {
                            // Yay! Found it, set values.
                            var changeFlag = _diffCallback.AreContentsTheSame(oldItem, newItem) ? FlagMovedNotChanged : FlagMovedChanged;
                            
                            // Once we process one of these, it will mark the other one as ignored.
                            _oldItemStatuses[positionX] = (positionY << FlagOffset) | changeFlag;
                            _newItemStatuses[positionY] = (positionX << FlagOffset) | changeFlag;
            
                            return;
                        }
                    }
            
                    positionY++;
                }
            
                positionY = diagonal.GetEndY();
            }
        }
        
        /// <summary>
        /// Find position mapping from old collection to new collection.
        /// If moves are requested, we'll also try to an N^2 search between additions and removals to find moves.
        /// </summary>
        private void FindMatchingItems()
        {
            foreach (var diagonal in _diagonals)
            {
                for (var offset = 0; offset < diagonal.Size; offset++)
                {
                    var positionX = diagonal.X + offset;
                    var positionY = diagonal.Y + offset;
                    
                    var changeFlag = _diffCallback.AreContentsTheSame(_oldArray[positionX], _newArray[positionY]) ? FlagNotChanged : FlagChanged;

                    _oldItemStatuses[positionX] = (positionY << FlagOffset) | changeFlag;
                    _newItemStatuses[positionY] = (positionX << FlagOffset) | changeFlag;
                }
            }
            
            // Now all matches are marked, lets look for moves.
            // Traverse each addition/removal from the end of the list, find matching additions/removals from before.
            if (_detectMoves)
                FindMoveMatches();
        }

        private void FindMoveMatches()
        {
            // For each removal, find matching addition.
            var positionX = 0;
            
            foreach (var diagonal in _diagonals)
            {
                while (positionX < diagonal.X)
                {
                    // There is a removal, find matching addition from the rest.
                    if (_oldItemStatuses[positionX] == 0)
                        FindMatchingAddition(positionX);
            
                    positionX++;
                }
            
                // Snap back for the next diagonal.
                positionX = diagonal.GetEndX();
            }
        }
        #endregion
        
        #region Constant Values
        /// <summary>
        /// Signifies an item not present in the collection.
        /// </summary>
        public const int NoPosition = -1;
        #endregion
        
        #region Private Constant Values
        /// <summary>
        /// Item stayed in the same location, but the contents changed.
        /// </summary>
        private const int FlagChanged = FlagNotChanged << 1;
        
        private const int FlagMask = (1 << FlagOffset) - 1;
        
        /// <summary>
        /// Item moved.
        /// </summary>
        private const int FlagMoved = FlagMovedChanged | FlagMovedNotChanged;
        
        /// <summary>
        /// Item has moved and contents have also changed.
        /// </summary>
        private const int FlagMovedChanged = FlagChanged << 1;
        
        /// <summary>
        /// Item has moved but contents stayed the same.
        /// </summary>
        private const int FlagMovedNotChanged = FlagMovedChanged << 1;
        
        /// <summary>
        /// Item stayed the same.
        /// </summary>
        private const int FlagNotChanged = 1;
        
        /// <summary>
        /// Since we are re-using the int arrays that were created in the Myers' step, we mask change flags.
        /// </summary>
        private const int FlagOffset = 4;
        #endregion
        
        #region Private Static Methods
        private static PostponedUpdate GetPostponedUpdate(ICollection<PostponedUpdate> postponedUpdates, int positionInCollection, bool removal)
        {
            if (postponedUpdates.FirstOrDefault(p => p.PositionInOwnerCollection == positionInCollection && p.Removal == removal) is not { } postponedUpdate)
                return null;
            
            postponedUpdates.Remove(postponedUpdate);

            foreach (var update in postponedUpdates)
                update.CurrentPosition += removal ? -1 : 1;

            return postponedUpdate;
        }
        #endregion
    }
}
