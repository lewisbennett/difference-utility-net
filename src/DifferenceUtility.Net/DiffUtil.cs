using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using DifferenceUtility.Net.Base;
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
            where TNew : class
            where TOld : class
        {
            var oldArray = oldCollection as TOld[] ?? oldCollection?.ToArray() ?? throw new ArgumentNullException(nameof(oldCollection));
            var newArray = newCollection as TNew[] ?? newCollection?.ToArray() ?? throw new ArgumentNullException(nameof(newCollection));

            if (diffCallback is null)
                throw new ArgumentNullException(nameof(diffCallback));
            
            var diagonals = new List<(int X, int Y)>();

            // Find all item matches (diagonals).
            for (var x = 0; x < oldArray.Length; x++)
            {
                for (var y = 0; y < newArray.Length; y++)
                {
                    if (!diffCallback.AreItemsTheSame(oldArray[x], newArray[y]))
                        continue;

                    // There should only be a maximum of 1 diagonal per column, so break once it has been found.
                    diagonals.Add((x, y));
                    
                    break;
                }
            }

            // Add the furthest possible point so that we can find a path from beginning to end.
            diagonals.Add((oldArray.Length, newArray.Length));
            
            var path = new List<int>();

            var currentX = 0;
            var currentY = 0;

            var diagonalIndex = 0;
            
            // Diagonals are removed after retrieving them so that, by the end of the below loop, the only
            // diagonals remaining are those that aren't used in the final path. These will be used later
            // for calculating move operations later, if detect moves is enabled.
            var currentTarget = diagonals[diagonalIndex];
            diagonals.RemoveAt(diagonalIndex);

            while (currentX < oldArray.Length || currentY < newArray.Length)
            {
                // Prioritise horizontal moves (removals) over vertical moves (insertions).
                while (currentX < currentTarget.X)
                {
                    path.Add((currentX << DiffOperation.Offset) | DiffOperation.Remove);
                    
                    currentX++;
                }

                while (currentY < currentTarget.Y)
                {
                    path.Add((currentY << DiffOperation.Offset) | DiffOperation.Insert);

                    currentY++;
                }
                
                // Diagonals are no longer possible once X or Y reaches the end.
                if (currentX == oldArray.Length || currentY == newArray.Length)
                    continue;
                
                // Add the diagonal.
                var diagonalPayload = DiffOperation.NoOperation;

                if (!diffCallback.AreContentsTheSame(oldArray[currentX], newArray[currentY]))
                    diagonalPayload |= DiffOperation.Update;
                
                path.Add(diagonalPayload);

                // Increment diagonally.
                currentX++;
                currentY++;
                
                // Get the next diagonal that is within range of the current coordinates.
                while (diagonalIndex < diagonals.Count)
                {
                    var diagonal = diagonals[diagonalIndex];

                    if (diagonal.X < currentX || diagonal.Y < currentY)
                    {
                        // Only increment the diagonal index if we're skipping this diagonal.
                        diagonalIndex++;
                        
                        continue;
                    }
                    
                    // Since we remove the diagonal, the index we're querying stays the same.
                    currentTarget = diagonal;
                    diagonals.RemoveAt(diagonalIndex);
                    
                    break;
                }
            }

            if (!detectMoves)
                return new DiffResult<TOld, TNew>(diffCallback, oldArray, newArray, path);

            // Moves are diagonals that aren't included in the path. As a result, they are represented as an
            // insert/remove operation, followed by the inverse later on in the path instructions. We have to
            // find these pairs and update their flags so that they're treated properly when applying the changes.
            
            foreach (var diagonal in diagonals)
            {
                int? xOperationIndex = null, yOperationIndex = null;
                
                for (var i = 0; i < path.Count; i++)
                {
                    var payload = path[i];
                    
                    // Skip this item if the payload already has the move flag.
                    // If an item has already been processed, what w as previously an encoded X coordinate will now be an encoded Y,
                    // coordinate and vice versa. If these new values match a non-processed value, this may select the wrong indexes.
                    if ((payload & DiffOperation.Move) != 0)
                        continue;
                    
                    // Nested loop search not required since we're querying both X and Y. With this approach, no matter
                    // which coordinate we find first, it is guaranteed that the next one will be after it in the path.

                    if (!xOperationIndex.HasValue && (payload & DiffOperation.Remove) != 0 && payload >> DiffOperation.Offset == diagonal.X)
                        xOperationIndex = i;

                    else if (!yOperationIndex.HasValue && (payload & DiffOperation.Insert) != 0 && payload >> DiffOperation.Offset == diagonal.Y)
                        yOperationIndex = i;
                    
                    if (xOperationIndex.HasValue && yOperationIndex.HasValue)
                        break;
                }
                
                // Both values are required to process a move operation.
                // Y operation index will always have a value if the X operation index does.
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
