using System;
using System.Collections.Generic;
using System.Linq;
using DifferenceUtility.Net.Base;

namespace DifferenceUtility.Net.Helper
{
    internal class DiffCalculator<TOld, TNew> : IComparer<Diagonal>
    {
        #region Fields
        private readonly bool _detectMoves;
        private readonly IDiffCallback<TOld, TNew> _diffCallback;
        private readonly TOld[] _oldArray;
        private readonly TNew[] _newArray;
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Calculates a <see cref="DiffResult{TOld,TNew}" /> for the provided data.
        /// </summary>
        public DiffResult<TOld, TNew> CalculateDiffResult()
        {
            var oldSize = _oldArray.Length;
            var newSize = _newArray.Length;

            var diagonals = new List<Diagonal>();

            // Instead of a recursive implementation, we keep our own stack to avoid potential stack overflow exceptions.
            var stack = new List<Range>
            {
                new(0, oldSize, 0, newSize)
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

                if (MidPoint(range, forward, backward) is not { } snake)
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
            diagonals.Sort(this);

            return new DiffResult<TOld, TNew>(_oldArray, _newArray, _diffCallback, diagonals, forward.Data, backward.Data, _detectMoves);
        }
    
        /// <inheritdoc />
        public int Compare(Diagonal x, Diagonal y)
        {
            return x.X - y.X;
        }
        #endregion
    
        #region Constructors
        public DiffCalculator(TOld[] oldArray, TNew[] newArray, IDiffCallback<TOld, TNew> diffCallback, bool detectMoves)
        {
            _detectMoves = detectMoves;
            _diffCallback = diffCallback;
            _oldArray = oldArray;
            _newArray = newArray;
        }
        #endregion
        
        #region Private Methods
        private Snake? Backward(Range range, CenteredArray forward, CenteredArray backward, int d)
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
                while (x > range.OldCollectionStart && y > range.NewCollectionStart && _diffCallback.AreItemsTheSame(_oldArray[x - 1], _newArray[y - 1]))
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
        
        private Snake? Forward(Range range, CenteredArray forward, CenteredArray backward, int d)
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
                while (x < range.OldCollectionEnd && y < range.NewCollectionEnd && _diffCallback.AreItemsTheSame(_oldArray[x], _newArray[y]))
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
        private Snake? MidPoint(Range range, CenteredArray forward, CenteredArray backward)
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
                if (Forward(range, forward, backward, d) is { } forwardSnake)
                    return forwardSnake;

                if (Backward(range, forward, backward, d) is { } backwardSnake)
                    return backwardSnake;
            }

            return null;
        }
        #endregion
    }
}
