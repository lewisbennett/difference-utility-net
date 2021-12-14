using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.Extensions;
using DifferenceUtility.Net.Helper;

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
    /// </summary>
    public static class DiffUtil
    {
        #region Public Methods
        /// <summary>
        /// <para>Calculates a set of update operations that can convert <paramref name="oldCollection" /> into <paramref name="newCollection" />.</para>
        /// <para>If your old and new collections are sorted by the same constraint and items never move (swap positions), you can disable move detection.</para>
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

            if (oldArray.Length == 0 && newArray.Length == 0)
                return DiffResult<TOld, TNew>.Empty();

            // Guaranteed to be no diagonals if either array is empty.
            if (oldArray.Length == 0 || newArray.Length == 0)
                return DiffResult<TOld, TNew>.NoDiagonals(diffCallback, oldArray, newArray);

            // The maximum number of diagonals is the length of the shortest collection.
            // Provide a capacity to reduce the number of re-allocations.
            var diagonals = new List<Diagonal>(Math.Min(oldArray.Length, newArray.Length));

            var longestCommonSubsequenceLength = 0;

            for (var x = 0; x < oldArray.Length; x++)
            {
                for (var y = 0; y < newArray.Length; y++)
                {
                    if (!diffCallback.AreItemsTheSame(oldArray[x], newArray[y]))
                        continue;

                    var score = 1;
                    
                    foreach (var diagonal in diagonals)
                    {
                        // If the current X/Y coordinates exist within range of the query coordinates,
                        // the query coordinates will both be less than the current X/Y coordinates.
                        if (diagonal.X < x && diagonal.Y < y)
                            score = Math.Max(score, diagonal.Score + 1);
                    }

                    diagonals.Add(new Diagonal
                    {
                        Score = score,
                        X = x,
                        Y = y
                    });

                    longestCommonSubsequenceLength = Math.Max(longestCommonSubsequenceLength, score);
                    
                    break;
                }
            }
            
            // If the longest common subsequence is zero then there are no diagonals in the matrix.
            if (longestCommonSubsequenceLength == 0)
                return DiffResult<TOld, TNew>.NoDiagonals(diffCallback, oldArray, newArray);

            // By now, we have every diagonal in the matrix, as well as their scores for calculating the shortest possible path.
            
            var path = new int[oldArray.Length + newArray.Length - longestCommonSubsequenceLength];
            
            var currentX = oldArray.Length - 1;
            var currentY = newArray.Length - 1;
            
            int GetCurrentPathIndex()
            {
                return currentX + currentY - longestCommonSubsequenceLength + 1;
            }
            
            // Construct the path.
            while (longestCommonSubsequenceLength > 0)
            {
                var diagonalIndex = 0;
                
                while (diagonalIndex < diagonals.Count)
                {
                    var diagonal = diagonals[diagonalIndex];

                    // Check that the diagonal is within backwards range of the current coordinates.
                    if (diagonal.Score != longestCommonSubsequenceLength || diagonal.X > currentX || diagonal.Y > currentY)
                    {
                        diagonalIndex++;
                        continue;
                    }

                    // Calculate the path between the current coordinates and the diagonal. We start with Y since we're working in reverse order.
                    while (currentY > diagonal.Y)
                    {
                        path[GetCurrentPathIndex()] = (currentY << DiffOperation.Offset) | DiffOperation.Insert;

                        currentY--;
                    }

                    while (currentX > diagonal.X)
                    {
                        path[GetCurrentPathIndex()] = (currentX << DiffOperation.Offset) | DiffOperation.Remove;
                        
                        currentX--;
                    }

                    // Now, handle the diagonal.
                    var diagonalPayload = 0;

                    if (!diffCallback.AreContentsTheSame(oldArray[currentX], newArray[currentY]))
                        diagonalPayload |= DiffOperation.Update;
                    
                    path[GetCurrentPathIndex()] = diagonalPayload;

                    currentX--;
                    currentY--;

                    longestCommonSubsequenceLength--;

                    // Remove the diagonal so it doesn't interfere with move calculation later on, if enabled.
                    diagonals.Remove(diagonal);

                    break;
                }
            }
            
            // Now we need to fill the gap between X0 Y0 and the first diagonal.
            while (currentY >= 0)
            {
                path[GetCurrentPathIndex()] = (currentY << DiffOperation.Offset) | DiffOperation.Insert;
                
                currentY--;
            }
            
            while (currentX >= 0)
            {
                path[GetCurrentPathIndex()] = (currentX << DiffOperation.Offset) | DiffOperation.Remove;
                
                currentX--;
            }
            
            if (!detectMoves)
                return new DiffResult<TOld, TNew>(diffCallback, oldArray, newArray, path);

            // Moves are diagonals that aren't included in the path. As a result, they are represented as an
            // insert/remove operation, followed by the inverse later on in the path instructions. We have to
            // find these pairs and update their flags so that they're treated properly when applying the changes.
            
            foreach (var diagonal in diagonals)
            {
                int? xOperationIndex = null, yOperationIndex = null;
                
                for (var i = 0; i < path.Length; i++)
                {
                    var operation = path[i];
                    
                    // Skip this item if the payload already has the move flag.
                    // If an item has already been processed, what was previously an encoded X coordinate will now be an encoded Y,
                    // coordinate and vice versa. If these new values match a non-processed value, this may select the wrong indexes.
                    if ((operation & DiffOperation.Move) != 0)
                        continue;
                    
                    // Nested loop search not required since we're querying both X and Y. With this approach, no matter
                    // which coordinate we find first, it is guaranteed that the next one will be after it in the path.

                    if (!xOperationIndex.HasValue && (operation & DiffOperation.Remove) != 0 && operation >> DiffOperation.Offset == diagonal.X)
                        xOperationIndex = i;

                    else if (!yOperationIndex.HasValue && (operation & DiffOperation.Insert) != 0 && operation >> DiffOperation.Offset == diagonal.Y)
                        yOperationIndex = i;
                    
                    if (xOperationIndex.HasValue && yOperationIndex.HasValue)
                        break;
                }
                
                // Both values are required to process a move operation.
                if (!xOperationIndex.HasValue || !yOperationIndex.HasValue)
                    continue;

                var xOperation = path[xOperationIndex.Value];
                var yOperation = path[yOperationIndex.Value];

                var x = xOperation >> DiffOperation.Offset;
                var y = yOperation >> DiffOperation.Offset;
                
                // Append additional flags to payload.
                var additionalFlags = DiffOperation.Move;

                if (!diffCallback.AreContentsTheSame(oldArray[x], newArray[y]))
                    additionalFlags |= DiffOperation.Update;

                // Moves require the X/Y coordinates to be inverted, but the flags should stay the same.
                path[xOperationIndex.Value] = (y << DiffOperation.Offset) | DiffOperation.Remove | additionalFlags;
                path[yOperationIndex.Value] = (x << DiffOperation.Offset) | DiffOperation.Insert | additionalFlags;
            }

            return new DiffResult<TOld, TNew>(diffCallback, oldArray, newArray, path);
        }
        #endregion
    }
}
