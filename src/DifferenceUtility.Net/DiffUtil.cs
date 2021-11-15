using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.Helper;
using Range = DifferenceUtility.Net.Helper.Range;

namespace DifferenceUtility.Net
{
    /// <summary>
    /// <para>DiffUtil is a utility class that can calculate the difference between two collections and output a set of update operations that converts
    /// the first collection into the second one.</para>
    /// 
    /// <para>It can be used to calculate updates for an <see cref="ObservableCollection{T}" />, and makes use of Insert, Move, Remove and Update operations
    /// to apply the changes in a fluid manner, ideal for UI based applications.</para>
    /// 
    /// <para>DiffUtil uses  Eugene W. Myers' difference algorithm to calculate the minimal number of updates to convert one collection to into another. Myers'
    /// algorithm does not handle items that are moved so DiffUtil runs a second pass on the result to detect them.</para>
    ///
    /// <para>Note that DiffUtil requires the collection to not mutate while in use. This generally means that both of the collections, and their elements,
    /// (or at least the properties of elements using in diffing) should not be modified directly. Instead, new collections should be provided any time
    /// content changes. It is common for collections passed to DiffUtil to share elements that have not mutated, so it is not strictly required to
    /// reload all data to use DiffUtil.</para>
    /// 
    /// <para>If the collections are large, this operations may take significant time, so you are advised to run this on a background thread, get the
    /// <see cref="DiffResult{TOld,TNew}" />, then apply it on the main thread.</para>
    /// 
    /// <para>This algorithm is optimized for space and uses O(N) space to find the minimal number of addition and removal operations between the two
    /// collections. It has O(N + D^2) expected time performance, where D is the length of the edit script.</para>
    /// 
    /// <para>If move detection is enabled, it takes an additional O(N^2) time, where N is the total number of added and removed items. If your
    /// collections are already sorted by the same constraint (e.e. a created timestamp for a collection of posts), you can disable move detection
    /// to improve performance.</para>
    /// 
    /// <para>The actual runtime of the algorithm significantly depends on the number of changes in the collection and the cost of your comparison methods.</para>
    /// 
    /// <para>Due to implementation constraints, the maximum size of the collection can be 2^26.</para>
    /// </summary>
    public static class DiffUtil
    {
        private class SnakeComparator : IComparer<Snake>
        {
            #region Public Methods
            /// <inheritdoc />
            public int Compare(Snake x, Snake y)
            {
                var compareX = x.X - y.X;

                return compareX == 0 ? x.Y - y.Y : compareX;
            }
            #endregion
        }

        #region Fields
        private static SnakeComparator _snakeComparator;
        #endregion
        
        #region Public Methods
        /// <summary>
        /// <para>Calculates a set of update operations that can convert <paramref name="oldCollection" /> into <paramref name="newCollection" />.</para>
        /// <para>If your old and new collections are sorted by the same constraint and items never move (swap positions), you can disable move detection
        /// which takes <c>O(N^2)</c> time, where N is the number of added, moved, or removed items.</para>
        /// </summary>
        /// <param name="diffCallback">A callback for calculating the difference between the provided collections.</param>
        /// <param name="detectMoves"><c>true</c> if DiffUtil should try to detect moved items, <c>false</c> otherwise.</param>
        /// <returns>A <see cref="DiffResult{T, T}" /> that contains the information about the edit sequence to convert the old collection into the new collection.</returns>
        public static DiffResult<TOld, TNew> CalculateDiff<TOld, TNew>(
            [NotNull] IEnumerable<TOld> oldCollection,
            [NotNull] IEnumerable<TNew> newCollection,
            [NotNull] IDiffCallback<TOld, TNew> diffCallback,
            bool detectMoves = true)
        {
            var oldArray = oldCollection as TOld[] ?? oldCollection?.ToArray() ?? throw new ArgumentNullException(nameof(oldCollection));
            var newArray = newCollection as TNew[] ?? newCollection?.ToArray() ?? throw new ArgumentNullException(nameof(newCollection));

            if (diffCallback is null)
                throw new ArgumentNullException(nameof(diffCallback));

            var oldSize = oldArray.Length;
            var newSize = newArray.Length;

            var snakes = new List<Snake>();

            // Instead of a recursive implementation, we keep our own stack to avoid potential stack overflow exceptions.
            var stack = new List<Range>
            {
                new()
                {
                    OldCollectionStart = 0,
                    OldCollectionEnd = oldSize,
                    NewCollectionStart = 0,
                    NewCollectionEnd = newSize
                }
            };

            var max = oldSize + newSize + Math.Abs(oldSize - newSize);

            var forwardBackwardArraySize = max * 2;
            
            // Allocate forward and backward K-lines. K-lines are diagonal lines in the matrix (see the paper for details).
            // These arrays lines keep the max reachable position for each k-line.
            var forward = new int[forwardBackwardArraySize];
            var backward = new int[forwardBackwardArraySize];
            
            // We pool the ranges to avoid allocations for each recursive call.
            var rangePool = new List<Range>();

            while (stack.Any())
            {
                var range = stack.Last();
                stack.Remove(range);

                var snakeResult = DiffPartial(diffCallback, oldArray, newArray, range.OldCollectionStart, range.OldCollectionEnd,
                    range.NewCollectionStart, range.NewCollectionEnd, forward, backward, max);

                if (snakeResult is not { } snake)
                {
                    rangePool.Add(range);
                    continue;
                }
                
                // Offset the snake to convert its coordinates from the Range's area to global.
                snake.X += range.OldCollectionStart;
                snake.Y += range.NewCollectionStart;
                
                // Safe to add the snake now as its values don't change after here.
                if (snake.Size > 0)
                    snakes.Add(snake);
                
                // Add new ranges for left and right.
                Range left;

                if (rangePool.Any())
                {
                    left = rangePool.Last();
                    rangePool.Remove(left);
                }
                else
                    left = new Range();

                left.OldCollectionStart = range.OldCollectionStart;
                left.NewCollectionStart = range.NewCollectionStart;

                if (snake.Reverse)
                {
                    left.OldCollectionEnd = snake.X;
                    left.NewCollectionEnd = snake.Y;
                }
                else if (snake.Removal)
                {
                    left.OldCollectionEnd = snake.X - 1;
                    left.NewCollectionEnd = snake.Y;
                }
                else
                {
                    left.OldCollectionEnd = snake.X;
                    left.NewCollectionEnd = snake.Y - 1;
                }
                
                stack.Add(left);
                
                // Re-use range for right.
                var right = range;

                if (!snake.Reverse)
                {
                    right.OldCollectionStart = snake.X + snake.Size;
                    right.NewCollectionStart = snake.Y + snake.Size;
                }
                else if (snake.Removal)
                {
                    right.OldCollectionStart = snake.X + snake.Size + 1;
                    right.NewCollectionStart = snake.Y + snake.Size;
                }
                else
                {
                    right.OldCollectionStart = snake.X + snake.Size;
                    right.NewCollectionStart = snake.Y + snake.Size + 1;
                }
                
                stack.Add(right);
            }
            
            // Sort snakes.
            snakes.Sort(_snakeComparator ??= new SnakeComparator());

            return new DiffResult<TOld, TNew>(oldArray, newArray, diffCallback, snakes, forward, backward, detectMoves);
        }
        #endregion
        
        #region Private Methods
        private static Snake? DiffPartial<TOld, TNew>(IDiffCallback<TOld, TNew> diffCallback, IReadOnlyList<TOld> oldArray, IReadOnlyList<TNew> newArray,
            int oldStart, int oldEnd, int newStart, int newEnd, int[] forward, int[] backward, int kOffset)
        {
            var oldSize = oldEnd - oldStart;
            var newSize = newEnd - newStart;

            if (oldSize < 1 || newSize < 1)
                return null;

            var delta = oldSize - newSize;
            var dLimit = (oldSize + newSize + 1) / 2;
            
            var forwardStart = kOffset - dLimit - 1;
            var backwardStart = forwardStart + delta;

            var count = kOffset + dLimit + 1;
            
            Array.Fill(forward, 0, forwardStart, count - forwardStart);
            Array.Fill(backward, oldSize, backwardStart, count + delta - backwardStart);

            var checkInForward = delta % 2 != 0;

            for (var d = 0; d <= dLimit; d++)
            {
                for (var k = -d; k <= d; k += 2)
                {
                    // Find forward path.
                    // We can reach K from K - 1, or K + 1. Check which one is further in the graph.
                    int x;
                    bool removal;

                    if (k == -d || k != d && forward[kOffset + k - 1] < forward[kOffset + k + 1])
                    {
                        x = forward[kOffset + k + 1];
                        removal = false;
                    }
                    else
                    {
                        x = forward[kOffset + k - 1] + 1;
                        removal = true;
                    }
                    
                    // Set Y based on X.
                    var y = x - k;
                    
                    // Move diagonal as long as items match.
                    while (x < oldSize && y < newSize && diffCallback.AreItemsTheSame(oldArray[oldStart + x], newArray[newStart + y]))
                    {
                        x++;
                        y++;
                    }

                    forward[kOffset + k] = x;
                    
                    if (!checkInForward || k < delta - d + 1 || k > delta + d - 1)
                        continue;
                    
                    var outBackward = backward[kOffset + k];
                    var outForward = forward[kOffset + k];
                    
                    if (outForward < outBackward)
                        continue;

                    return new Snake
                    {
                        Removal = removal,
                        Reverse = false,
                        Size = outForward - outBackward,
                        X = outBackward,
                        Y = outBackward - k
                    };
                }

                for (var k = -d; k <= d; k += 2)
                {
                    // Find reverse path at K + delta, in reverse.
                    var backwardK = k + delta;

                    int x;
                    bool removal;

                    if (backwardK == d + delta || backwardK != -d + delta && backward[kOffset + backwardK - 1] < backward[kOffset + backwardK + 1])
                    {
                        x = backward[kOffset + backwardK - 1];
                        removal = false;
                    }
                    else
                    {
                        x = backward[kOffset + backwardK + 1] - 1;
                        removal = true;
                    }
                    
                    // Set Y based on X.
                    var y = x - backwardK;
                    
                    // Move diagonal as long as items match.
                    while (x > 0 && y > 0 && diffCallback.AreItemsTheSame(oldArray[oldStart + x - 1], newArray[newStart + y - 1]))
                    {
                        x--;
                        y--;
                    }

                    backward[kOffset + backwardK] = x;

                    if (checkInForward || k + delta < -d || k + delta > d)
                        continue;

                    var outBackward = backward[kOffset + backwardK];
                    var outForward = forward[kOffset + backwardK];
                        
                    if (outForward < outBackward)
                        continue;

                    return new Snake
                    {
                        Removal = removal,
                        Reverse = true,
                        Size = outForward - outBackward,
                        X = outBackward,
                        Y = outBackward - backwardK
                    };
                }
            }

            throw new InvalidOperationException("DiffUtil hit an unexpected case while trying to calculate the optimal path."
                + "Please make sure that your data is not changing during the diff calculation.");
        }
        #endregion
    }
}
