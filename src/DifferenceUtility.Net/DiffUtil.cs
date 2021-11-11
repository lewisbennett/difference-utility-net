using System;
using System.Collections.Generic;
using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.Helper;
using DifferenceUtility.Net.Schema;

namespace DifferenceUtility.Net
{
    public static class DiffUtil
    {
        #region Public Methods
        /// <summary>
        /// Calculates the difference between <paramref name="oldCollection" /> and <paramref name="newCollection" />.
        /// </summary>
        /// <param name="diffCallback">A callback for calculating the difference between the provided collections.</param>
        /// <returns>A <see cref="DiffResult{T, T}" /> with configured instructions.</returns>
        public static DiffResult<TOld, TNew> CalculateDiff<TOld, TNew>(IEnumerable<TOld> oldCollection, IEnumerable<TNew> newCollection, IDiffCallback<TOld, TNew> diffCallback)
        {
            var waypoints = new DiffCalculator<TOld, TNew>(oldCollection, newCollection, diffCallback).CalculatePath(out var oldArray, out var newArray);

            var diffInstructions = new List<(TOld, TNew, DiffStatus)>();
            
            foreach (var (current, previous) in MakePairsWithNext(waypoints))
            {
                var status = GetDiffStatus(current, previous);

                TOld old = default;
                TNew @new = default;

                switch (status)
                {
                    case DiffStatus.Deleted:
                        old = oldArray[current.X - 1];
                        break;
                    
                    case DiffStatus.Equal:
                        old = oldArray[current.X - 1];
                        @new = newArray[current.Y - 1];
                        break;
                    
                    case DiffStatus.Inserted:
                        @new = newArray[current.Y - 1];
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                diffInstructions.Add((old, @new, status));
            }

            return new DiffResult<TOld, TNew>(diffCallback, diffInstructions);
        }
        #endregion
        
        #region Private Methods
        private static DiffStatus GetDiffStatus(Point current, Point previous)
        {
            if (current.X != previous.X && current.Y != previous.Y)
                return DiffStatus.Equal;
            
            if (current.X != previous.X)
                return DiffStatus.Deleted;
            
            if (current.Y != previous.Y)
                return DiffStatus.Inserted;

            throw new Exception();
        }
        
        private static IEnumerable<(Point Current, Point Previous)> MakePairsWithNext(IEnumerable<Point> waypoints)
        {
            using var enumerator = waypoints.GetEnumerator();

            if (!enumerator.MoveNext())
                yield break;

            var previous = enumerator.Current;

            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;

                yield return (current, previous);

                previous = current;
            }
        }
        #endregion
    }
}
