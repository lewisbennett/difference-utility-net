using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.Helper;

namespace DifferenceUtility.Net
{
    public static class DiffUtil
    {
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

            return new DiffCalculator<TOld, TNew>(oldArray, newArray, diffCallback, detectMoves).CalculateDiffResult();
        }
        #endregion
    }
}
