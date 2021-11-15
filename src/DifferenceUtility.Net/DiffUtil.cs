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
        private class DiagonalComparator : IComparer<Diagonal>
        {
            #region Public Methods
            /// <inheritdoc />
            public int Compare(Diagonal x, Diagonal y)
            {
                return x.X - y.X;
            }
            #endregion
        }
        
        #region Fields
        private static DiagonalComparator _diagonalComparator;
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

            var diagonals = new List<Diagonal>();

            // Instead of a recursive implementation, we keep our own stack to avoid potential stack overflow exceptions.
            var stack = new List<Range>
            {
                new()
                {
                    NewCollectionEnd = newSize,
                    NewCollectionStart = 0,
                    OldCollectionEnd = oldSize,
                    OldCollectionStart = 0
                }
            };

            var max = (oldSize + newSize + 1) / 2;

            var centeredArraySize = max * 2 + 1;
            
            // Allocate forward and backward K-lines. K-lines are diagonal lines in the matrix (see the paper for details).
            // These arrays lines keep the max reachable position for each k-line.
            var forward = new CenteredArray(centeredArraySize);
            var backward = new CenteredArray(centeredArraySize);
            
            // We pool the ranges to avoid allocations for each recursive call.
            var rangePool = new List<Range>();

            while (stack.Any())
            {
                var range = stack.Last();
                stack.Remove(range);

                if (MidPoint(diffCallback, oldArray, newArray, range, forward, backward) is not { } snake)
                {
                    rangePool.Add(range);
                    continue;
                }
                
                // If it has a diagonal, save it.
                if (snake.DiagonalSize() > 0)
                    diagonals.Add(snake.ToDiagonal());
                
                // Add new ranges for left and right.
                Range left;

                if (rangePool.Any())
                {
                    left = rangePool.Last();
                    rangePool.Remove(left);
                }
                else
                    left = new Range();

                left.NewCollectionEnd = snake.StartY;
                left.NewCollectionStart = range.NewCollectionStart;
                left.OldCollectionEnd = snake.StartX;
                left.OldCollectionStart = range.OldCollectionStart;
                
                stack.Add(left);
                
                // Re-use range for right.
                var right = range;
                
                right.OldCollectionEnd = range.OldCollectionEnd;
                right.OldCollectionStart = snake.EndX;
                right.NewCollectionEnd = range.NewCollectionEnd;
                right.NewCollectionStart = snake.EndY;
                
                stack.Add(right);
            }
            
            // Sort snakes.
            diagonals.Sort(_diagonalComparator ??= new DiagonalComparator());

            return new DiffResult<TOld, TNew>(diffCallback, oldArray, newArray, diagonals, forward.BackingData, backward.BackingData, detectMoves);
        }
        #endregion
        
        #region Private Methods
        private static Snake? Backward<TOld, TNew>(IDiffCallback<TOld, TNew> diffCallback, IReadOnlyList<TOld> oldArray, IReadOnlyList<TNew> newArray,
            Range range, CenteredArray forward, CenteredArray backward, int d)
        {
            var oldCollectionSize = range.GetOldCollectionSize();
            var newCollectionSize = range.GetNewCollectionSize();

            var checkForSnake = (oldCollectionSize - newCollectionSize) % 2 == 0;
            var delta = oldCollectionSize - newCollectionSize;
            
            // Same as forward, but we go backwards from end of the collections to the beginning.
            // This also means we'll try to optimize for minimizing X instead of maximizing it.
            for (var k = -d; k <= d; k += 2)
            {
                // We either came from D-1, K-1 OR D-1, K+1.
                // As we move in steps of 2, array always holds both current and previous D values.
                // K = X - Y and each array value hold the min X, Y = X - K.
                // When X's are equal, we prioritize deletion over insertion.

                int startX, x;
                
                // Picking K + 1, decrementing Y (by simply not decrementing X).
                if (k == -d || k != d && backward.Get(k + 1) < backward.Get(k - 1))
                    x = startX = backward.Get(k + 1);

                else
                {
                    // Picking K - 1, decrementing X.
                    startX = backward.Get(k - 1);
                    x = startX - 1;
                }

                var y = range.NewCollectionEnd - (range.OldCollectionEnd - x - k);
                
                var startY = d == 0 || x != startX ? y : y + 1;
                
                // Now find snake size.
                while (x > range.OldCollectionStart && y > range.NewCollectionStart && diffCallback.AreItemsTheSame(oldArray[x - 1], newArray[y - 1]))
                {
                    x--;
                    y--;
                }
                
                // Now we have furthest point, record it (min X).
                backward.Set(k, x);

                if (!checkForSnake)
                    continue;

                // See if we did pass over a backwards array.
                // Mapping function: delta - k.
                var forwardsK = delta - k;
                
                // If forwards K is calculated it passed me, found match.
                if (forwardsK >= -d && forwardsK <= d && forward.Get(forwardsK) >= x)
                {
                    // Match.
                    return new Snake
                    {
                        // Assignment are reverse since we are a reverse snake.
                        EndX = startX,
                        EndY = startY,
                        Reverse = true,
                        StartX = x,
                        StartY = y
                    };
                }
            }

            return null;
        }
        
        private static Snake? Forward<TOld, TNew>(IDiffCallback<TOld, TNew> diffCallback, IReadOnlyList<TOld> oldArray, IReadOnlyList<TNew> newArray,
            Range range, CenteredArray forward, CenteredArray backward, int d)
        {
            var oldCollectionSize = range.GetOldCollectionSize();
            var newCollectionSize = range.GetNewCollectionSize();
            
            var checkForSnake = Math.Abs(oldCollectionSize - newCollectionSize) % 2 == 1;
            var delta = oldCollectionSize - newCollectionSize;

            for (var k = -d; k <= d; k += 2)
            {
                // We either come from D-1, K-1, OR D-1, K+1.
                // As we move in steps of 2, array always holds both current and previous D values.
                // K = X - Y and each array value holds the max X, Y = X - K.

                int startX, x;

                // Picking K + 1, incrementing Y (by simply not incrementing X).
                if (k == -d || k != d && forward.Get(k + 1) > forward.Get(k - 1))
                    x = startX = forward.Get(k + 1);

                else
                {
                    // Picking K - 1, incrementing X.
                    startX = forward.Get(k - 1);
                    x = startX + 1;
                }

                var y = range.NewCollectionStart + (x - range.OldCollectionStart) - k;
                
                var startY = d == 0 || x != startX ? y : y - 1;
                
                // Now find snake size.
                while (x < range.OldCollectionEnd && y < range.NewCollectionEnd && diffCallback.AreItemsTheSame(oldArray[x], newArray[y]))
                {
                    x++;
                    y++;
                }
                
                // Now we have the furthest reaching X, record it.
                forward.Set(k, x);

                if (!checkForSnake)
                    continue;

                // See if we did pass over a backwards array.
                // Mapping function: delta - k.
                var backwardsK = delta - k;
                
                // If backwards K is calculated and it passed me, found match.
                if (backwardsK >= -d + 1 && backwardsK <= d - 1 && backward.Get(backwardsK) <= x)
                {
                    // Match.
                    return new Snake
                    {
                        EndX = x,
                        EndY = y,
                        Reverse = false,
                        StartX = startX,
                        StartY = startY
                    };
                }
            }

            return null;
        }
        
        /// <summary>
        /// Finds a middle snake in the given range.
        /// </summary>
        private static Snake? MidPoint<TOld, TNew>(IDiffCallback<TOld, TNew> diffCallback, IReadOnlyList<TOld> oldArray, IReadOnlyList<TNew> newArray,
            Range range, CenteredArray forward, CenteredArray backward)
        {
            var oldCollectionSize = range.GetOldCollectionSize();
            var newCollectionSize = range.GetNewCollectionSize();
            
            if (oldCollectionSize < 1 || newCollectionSize < 1)
                return null;

            var max = (oldCollectionSize + newCollectionSize + 1) / 2;
            
            forward.Set(1, range.OldCollectionStart);
            backward.Set(1, range.OldCollectionEnd);

            for (var d = 0; d < max; d++)
            {
                if (Forward(diffCallback, oldArray, newArray, range, forward, backward, d) is { } forwardSnake)
                    return forwardSnake;

                if (Backward(diffCallback, oldArray, newArray, range, forward, backward, d) is { } backwardSnake)
                    return backwardSnake;
            }

            return null;
        }
        #endregion
    }
}
