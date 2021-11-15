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
        private readonly IList<Snake> _snakes;
        private readonly IDiffCallback<TOld, TNew> _diffCallback;
        private readonly TNew[] _newArray;
        private readonly int[] _newItemStatuses, _oldItemStatuses;
        private readonly TOld[] _oldArray;
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Dispatches the update events to the given collection.
        /// </summary>
        /// <param name="observableCollection">A collection which is displaying the old collection, and will start displaying the new collection.</param>
        public void DispatchUpdatesTo([NotNull] ObservableCollection<TOld> observableCollection)
        {
            DispatchUpdatesTo(new ObservableCollectionUpdateCallback<TOld, TNew>(_diffCallback,
                observableCollection ?? throw new ArgumentNullException(nameof(observableCollection)), _newArray));
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
            
            if (updateCallback is not BatchingCollectionUpdateCallback batchingUpdateCallback)
                batchingUpdateCallback = new BatchingCollectionUpdateCallback(updateCallback);
            
            // These are add/remove operations that are converted to moves. We track their positions until their respective update operations are processed.
            var postponedUpdates = new List<PostponedUpdate>();

            var oldPosition = _oldArray.Length;
            var newPosition = _newArray.Length;

            for (var snakeIndex = _snakes.Count - 1; snakeIndex >= 0; snakeIndex--)
            {
                var snake = _snakes[snakeIndex];

                var endX = snake.X + snake.Size;
                var endY = snake.Y + snake.Size;

                if (endX < oldPosition)
                    DispatchRemovals(postponedUpdates, batchingUpdateCallback, endX, oldPosition - endX, endX);

                if (endY < newPosition)
                    DispatchAdditions(postponedUpdates, batchingUpdateCallback, endX, newPosition - endY, endY);

                for (var i = snake.Size - 1; i >= 0; i--)
                {
                    if ((_oldItemStatuses[snake.X + i] & FlagMask) == FlagChanged)
                        batchingUpdateCallback.OnChanged(snake.X + i, snake.Y + i,1);
                }

                oldPosition = snake.X;
                newPosition = snake.Y;
            }
            
            batchingUpdateCallback.DispatchLastEvent();
        }
        #endregion
        
        #region Constructors
        internal DiffResult(TOld[] oldArray, TNew[] newArray, IDiffCallback<TOld, TNew> diffCallback, IList<Snake> snakes, int[] oldItemStatuses, int[] newItemStatuses, bool detectMoves)
        {
            _detectMoves = detectMoves;
            _snakes = snakes;
            _diffCallback = diffCallback;
            _newArray = newArray;
            _newItemStatuses = newItemStatuses;
            _oldArray = oldArray;
            _oldItemStatuses = oldItemStatuses;
            
            Array.Fill(_oldItemStatuses, 0);
            Array.Fill(_newItemStatuses, 0);
            
            AddRootSnake();
            FindMatchingItems();
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// We always add a snake to 0, 0 so that we can run loops from end to beginning and be done when we run out of snakes.
        /// </summary>
        private void AddRootSnake()
        {
            if (_snakes.FirstOrDefault() is not { X: 0, Y: 0 })
            {
                _snakes.Insert(0, new Snake
                {
                    Removal = false,
                    Reverse = false,
                    Size = 0,
                    X = 0,
                    Y = 0
                });
            }
        }

        private void DispatchAdditions(List<PostponedUpdate> postponedUpdates, ICollectionUpdateCallback updateCallback, int start, int count, int globalIndex)
        {
            if (!_detectMoves)
            {
                updateCallback.OnInserted(start, count);
                return;
            }

            for (var i = count - 1; i >= 0; i--)
            {
                var status = _newItemStatuses[globalIndex + 1] & FlagMask;

                switch (status)
                {
                    // Real addition.
                    case 0:
                        
                        updateCallback.OnInserted(start, 1);

                        foreach (var postponedUpdate in postponedUpdates)
                            postponedUpdate.CurrentPosition++;
                        
                        break;
                    
                    case FlagMovedChanged:
                    case FlagMovedNotChanged:

                        var position = _newItemStatuses[globalIndex + i] >> FlagOffset;

                        var update = RemovePostponedUpdate(postponedUpdates, position, true);
                        
                        // The item was moved from that position.
                        updateCallback.OnMoved(update.CurrentPosition, start);
                        
                        // Also, dispatch a change.
                        if (status == FlagMovedChanged)
                            updateCallback.OnChanged(start, globalIndex + 1, 1);
                        
                        break;
                    
                    // Ignoring this.
                    case FlagIgnore:
                        postponedUpdates.Add(new PostponedUpdate(globalIndex + i, start, false));
                        break;
                    
                    default:
                        throw new InvalidOperationException($"Unknown flag for position: {globalIndex + i}: {Convert.ToString(status, 2)}");
                }
            }
        }

        private void DispatchRemovals(List<PostponedUpdate> postponedUpdates, ICollectionUpdateCallback updateCallback, int start, int count, int globalIndex)
        {
            if (!_detectMoves)
            {
                updateCallback.OnRemoved(start, count);
                return;
            }

            for (var i = count - 1; i >= 0; i--)
            {
                var status = _oldItemStatuses[globalIndex + i] & FlagMask;

                switch (status)
                {
                    // Real removal.
                    case 0:

                        updateCallback.OnRemoved(start + i, 1);

                        foreach (var postponedUpdate in postponedUpdates)
                            postponedUpdate.CurrentPosition--;
                        
                        break;
                    
                    case FlagMovedChanged:
                    case FlagMovedNotChanged:

                        var position = _oldItemStatuses[globalIndex + i] >> FlagOffset;

                        var update = RemovePostponedUpdate(postponedUpdates, position, false);
                        
                        // The item was moved to that position. We do -1 because this is a move not add and removing current item offsets the target move by 1.
                        updateCallback.OnMoved(start + i, update.CurrentPosition - 1);
                        
                        // Also, dispatch a change.
                        if (status == FlagMovedChanged)
                            updateCallback.OnChanged(update.CurrentPosition - 1, position, 1);
                        
                        break;
                    
                    // Ignoring this.
                    case FlagIgnore:
                        postponedUpdates.Add(new PostponedUpdate(globalIndex + i, start + i, true));
                        break;
                    
                    default:
                        throw new InvalidOperationException($"Unknown flag for position: {globalIndex + i}: {Convert.ToString(status, 2)}");
                }
            }
        }
        
        private void FindAddition(int x, int y, int snakeIndex)
        {
            // Already set by a latter item if evaluates to false.
            if (_oldItemStatuses[x - 1] == 0)
                FindMatchingItem(x, y, snakeIndex, false);
        }

        /// <summary>
        /// Finds a matching item that is before the given coordinates in the matrix (before: left and above).
        /// </summary>
        /// <param name="x">The X coordinate in the matrix (position in the old collection).</param>
        /// <param name="y">The Y coordinate in the matrix (position in the new collection).</param>
        /// <param name="snakeIndex">The current snake index.</param>
        /// <param name="removal"><c>true</c> if we are looking for a removal, <c>false</c> otherwise.</param>
        /// <returns><c>true</c> if such item is found, <c>false</c> otherwise.</returns>
        private bool FindMatchingItem(int x, int y, int snakeIndex, bool removal)
        {
            int currentX, currentY, myItemPosition;

            if (removal)
            {
                myItemPosition = y - 1;
                currentX = x;
                currentY = y - 1;
            }
            else
            {
                myItemPosition = x - 1;
                currentX = x - 1;
                currentY = y;
            }

            for (var i = snakeIndex; i >= 0; i--)
            {
                var snake = _snakes[i];
                
                var endX = snake.X + snake.Size;
                var endY = snake.Y + snake.Size;

                if (removal)
                {
                    // Check removals for a match.
                    for (var position = currentX - 1; position >= endX; position--)
                    {
                        var oldItem = _oldArray[position];
                        var newItem = _newArray[myItemPosition];

                        if (!_diffCallback.AreItemsTheSame(oldItem, newItem))
                            continue;

                        // Found!
                        var changeFlag = _diffCallback.AreContentsTheSame(oldItem, newItem) ? FlagMovedNotChanged : FlagMovedChanged;

                        _oldItemStatuses[position] = (myItemPosition << FlagOffset) | changeFlag;
                        _newItemStatuses[myItemPosition] = (position << FlagOffset) | FlagIgnore;

                        return true;
                    }
                }
                else
                {
                    // Check for addition for a match.
                    for (var position = currentY - 1; position >= endY; position--)
                    {
                        var oldItem = _oldArray[myItemPosition];
                        var newItem = _newArray[position];

                        if (!_diffCallback.AreItemsTheSame(oldItem, newItem))
                            continue;

                        // Found!
                        var changeFlag = _diffCallback.AreContentsTheSame(oldItem, newItem) ? FlagMovedNotChanged : FlagMovedChanged;

                        _oldItemStatuses[x - 1] = (position << FlagOffset) | FlagIgnore;
                        _newItemStatuses[position] = ((x - 1) << FlagOffset) | changeFlag;

                        return true;
                    }
                }

                currentX = snake.X;
                currentY = snake.Y;
            }

            return false;
        }
        
        /// <summary>
        /// <para>This method traverses each addition/removal and tries to match it to a previous addition/removal. This is how we detect move operations.</para>
        /// <para>This class also flags whether an item has been changed or not.</para>
        /// <para>DiffUtil does this pre-processing so that if it is running on a big collection, it can be moved to a background thread where most of the
        /// expensive stuff will be calculated and kept in the statuses maps. DiffResult uses this pre-calculated information while dispatched the updates
        /// (which is probably being called from the main thread).</para>
        /// </summary>
        private void FindMatchingItems()
        {
            var oldPosition = _oldArray.Length;
            var newPosition = _newArray.Length;
            
            // Traverse the matrix from bottom right to 0, 0.
            for (var i = _snakes.Count - 1; i >= 0; i--)
            {
                var snake = _snakes[i];

                var endX = snake.X + snake.Size;
                var endY = snake.Y + snake.Size;

                if (_detectMoves)
                {
                    while (oldPosition > endX)
                    {
                        // This is a removal. Check remaining snakes to see if this was added before.
                        FindAddition(oldPosition, newPosition, i);
                        
                        oldPosition--;
                    }

                    while (newPosition > endY)
                    {
                        // This is an addition. Check remaining snakes to see if this was removed before.
                        FindRemoval(oldPosition, newPosition, i);

                        newPosition--;
                    }
                }

                for (var j = 0; j < snake.Size; j++)
                {
                    // Matching items. Check if it has changed or not.
                    var oldItemPosition = snake.X + j;
                    var newItemPosition = snake.Y + j;

                    var changeFlag = _diffCallback.AreContentsTheSame(_oldArray[oldItemPosition], _newArray[newItemPosition]) ? FlagNotChanged : FlagChanged;

                    _oldItemStatuses[oldItemPosition] = (newItemPosition << FlagOffset) | changeFlag;
                    _newItemStatuses[newItemPosition] = (oldItemPosition << FlagOffset) | changeFlag;
                }

                oldPosition = snake.X;
                newPosition = snake.Y;
            }
        }

        private void FindRemoval(int x, int y, int snakeIndex)
        {
            // Already set by a latter item if evaluates to false.
            if (_newItemStatuses[y - 1] == 0)
                FindMatchingItem(x, y, snakeIndex, true);
        }
        #endregion
        
        #region Private Constant Values
        /// <summary>
        /// Item stayed in the same location, but the contents changed.
        /// </summary>
        private const int FlagChanged = FlagNotChanged << 1;

        /// <summary>
        /// <para>Ignore this update.</para>
        /// <para>If this is an addition from the new list, it means the item is actually from from an earlier position
        /// and its move will be dispatched when we process the matching removal from the old list.</para>
        /// <para>If this is a removal from the old list, it means the item is actually added back to an earlier index
        /// in the new list and we'll dispatch its move when we are processing that addition.</para>
        /// </summary>
        private const int FlagIgnore = FlagMovedNotChanged << 1;
        
        private const int FlagMask = (1 << FlagOffset) - 1;
        
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
        private const int FlagOffset = 5;
        #endregion
        
        #region Private Static Methods
        private static PostponedUpdate RemovePostponedUpdate(List<PostponedUpdate> postponedUpdates, int position, bool removal)
        {
            if (postponedUpdates.FirstOrDefault(p => p.PositionInOwnerCollection == position && p.Removal == removal) is not { } postponedUpdate)
                return null;

            postponedUpdates.Remove(postponedUpdate);

            // Offset other operations since they swapped positions.
            foreach (var update in postponedUpdates)
                update.CurrentPosition += removal ? 1 : -1;
            
            return postponedUpdate;
        }
        #endregion
    }
}
