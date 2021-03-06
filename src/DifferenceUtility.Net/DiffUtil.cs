using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.Helper;

namespace DifferenceUtility.Net;

/// <summary>
///     <para>
///         DiffUtil is a utility class that can calculate the difference between two collections and output a set of
///         update operations that converts the first collection into the second one.
///     </para>
///     <para>
///         It can be used to calculate updates for an <see cref="ObservableCollection{T}" />, and makes use of Insert,
///         Move, Remove and Update operations to apply the changes in a fluid manner, ideal for UI based applications.
///     </para>
///     <para>
///         DiffUtil uses  Eugene W. Myers' difference algorithm to calculate the minimal number of updates to convert one
///         collection to another. Myers' algorithm does not handle items that are moved so DiffUtil runs a second
///         pass on the result to detect them.
///     </para>
///     <para>
///         Note that DiffUtil requires the collection to not mutate while in use. This generally means that both of the
///         collections, and their elements, (or at least the properties of elements using in diffing) should not be
///         modified directly. Instead, new collections should be provided any time content changes. It is common for
///         collections passed to DiffUtil to share elements that have not mutated, so it is not strictly required to
///         reload all data to use DiffUtil.
///     </para>
///     <para>
///         If the collections are large, this operation may take significant time, so you are advised to run this on a
///         background thread, get the <see cref="DiffResult{TOld,TNew}" />, then apply it on the main thread.
///     </para>
/// </summary>
public static class DiffUtil
{
    #region Public Methods
    /// <summary>
    ///     <para>
    ///         Calculates a set of modification operations that can convert <paramref name="sourceCollection" /> into
    ///         <paramref name="destinationCollection" />.
    ///     </para>
    ///     <para>
    ///         If your source and destination collections are sorted by the same constraint and items never move (swap
    ///         positions), you can disable move detection.
    ///     </para>
    /// </summary>
    /// <param name="sourceCollection">The source collection containing the current data.</param>
    /// <param name="destinationCollection">The destination collection containing the new data.</param>
    /// <param name="diffCallback">A callback for calculating the difference between the provided collections.</param>
    /// <param name="detectMoves"><c>true</c> if DiffUtil should try to detect moved items, <c>false</c> otherwise.</param>
    /// <returns>
    ///     A <see cref="DiffResult{T, T}" /> that contains the information about the edit sequence to convert the old
    ///     collection into the new collection.
    /// </returns>
    public static DiffResult<TSource, TDestination> CalculateDiff<TSource, TDestination>(
        [NotNull] IEnumerable<TSource> sourceCollection,
        [NotNull] IEnumerable<TDestination> destinationCollection,
        [NotNull] IDiffCallback<TSource, TDestination> diffCallback,
        bool detectMoves = true)
    {
        var sourceArray = sourceCollection as TSource[] ?? sourceCollection?.ToArray() ?? throw new ArgumentNullException(nameof(sourceCollection));
        var destinationArray = destinationCollection as TDestination[] ?? destinationCollection?.ToArray() ?? throw new ArgumentNullException(nameof(destinationCollection));

        if (diffCallback is null)
            throw new ArgumentNullException(nameof(diffCallback));

        if (sourceArray.Length == 0 && destinationArray.Length == 0)
            return DiffResult<TSource, TDestination>.Empty();

        // Guaranteed to be no diagonals if either array is empty.
        if (sourceArray.Length == 0 || destinationArray.Length == 0)
            return DiffResult<TSource, TDestination>.NoDiagonals(diffCallback, sourceArray, destinationArray);

        // When calculating the path, we use a mixture of global index, X, and Y coordinates.
        // The global index represents the position of the coordinates within the diff matrix.
        // Global index = X * destination length + Y
        // X = global index / destination length (cast to integer to avoid decimal places)
        // Y = global index % destination length

        List<(int GlobalIndex, int Score)> diagonals = null;

        var longestCommonSubsequenceLength = 0;

        // Loop through the entire diff matrix to find matching items.
        var matrixArea = sourceArray.Length * destinationArray.Length;

        for (var globalIndex = 0; globalIndex < matrixArea; globalIndex++)
        {
            if (!diffCallback.AreItemsTheSame(sourceArray[globalIndex / destinationArray.Length], destinationArray[globalIndex % destinationArray.Length]))
                continue;

            if (diagonals is null)
            {
                // The maximum possible number of diagonals is the length of the shortest provided collection.
                diagonals = new List<(int GlobalIndex, int Score)>(Math.Min(sourceArray.Length, destinationArray.Length))
                {
                    (globalIndex, 1)
                };

                longestCommonSubsequenceLength = 1;

                continue;
            }

            // The score represents the number of diagonals that exist before the current
            // diagonal, that can also lead to the current diagonal when the path is followed.
            var score = 1;

            for (var diagonalIndex = 0; diagonalIndex < diagonals.Count; diagonalIndex++)
            {
                var diagonal = diagonals[diagonalIndex];

                // Query the diagonal's global index to see if it could possibly lead to the current diagonal.
                if (diagonal.GlobalIndex < globalIndex - destinationArray.Length && diagonal.GlobalIndex % destinationArray.Length < globalIndex % destinationArray.Length)
                    score = Math.Max(score, diagonal.Score + 1);
            }

            diagonals.Add((globalIndex, score));

            longestCommonSubsequenceLength = Math.Max(longestCommonSubsequenceLength, score);
        }

        if (diagonals is null)
            return DiffResult<TSource, TDestination>.NoDiagonals(diffCallback, sourceArray, destinationArray);

        // By now, we have every diagonal in the matrix, as well as their scores for calculating the shortest possible path.

        var path = new int[sourceArray.Length + destinationArray.Length - longestCommonSubsequenceLength];

        var currentX = sourceArray.Length - 1;
        var currentY = destinationArray.Length - 1;

        int GetCurrentPathIndex()
        {
            return currentX + currentY - longestCommonSubsequenceLength + 1;
        }

        // Construct the path.
        for (var diagonalIndex = diagonals.Count - 1; diagonalIndex >= 0; diagonalIndex--)
        {
            var diagonal = diagonals[diagonalIndex];

            if (diagonal.Score != longestCommonSubsequenceLength)
                continue;

            var diagonalX = diagonal.GlobalIndex / destinationArray.Length;

            if (diagonalX > currentX)
                continue;

            var diagonalY = diagonal.GlobalIndex % destinationArray.Length;

            if (diagonalY > currentY)
                continue;

            // Calculate the path between the current coordinates and the diagonal.
            while (currentY > diagonalY)
            {
                path[GetCurrentPathIndex()] = (currentY << DiffOperation.Offset) | DiffOperation.Insert;

                currentY--;
            }

            while (currentX > diagonalX)
            {
                path[GetCurrentPathIndex()] = (currentX << DiffOperation.Offset) | DiffOperation.Remove;

                currentX--;
            }

            // Now handle the diagonal.
            if (!diffCallback.AreContentsTheSame(sourceArray[currentX], destinationArray[currentY]))
                path[GetCurrentPathIndex()] = DiffOperation.Update;

            currentX--;
            currentY--;

            longestCommonSubsequenceLength--;

            // If moves are enabled, removing the diagonal helps to speed up move detection later.
            // If moves are disabled, there is a speed advantage by not removing the diagonal.
            if (detectMoves)
                diagonals.Remove(diagonal);
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

        // If move detection is disabled, we can return here to avoid the extra pass required to modify move operations.
        if (!detectMoves)
            return new DiffResult<TSource, TDestination>(diffCallback, sourceArray, destinationArray, path);

        // Moves are diagonals that aren't included in the path. As a result, they are represented as an
        // insert/remove operation, followed by the inverse later on in the path instructions. We have to
        // find these pairs and update their flags so that they're treated properly when applying the changes.

        var currentOperationIndex = 0;

        for (var diagonalIndex = 0; diagonalIndex < diagonals.Count; diagonalIndex++)
        {
            // X/Y operation indexes can never be retrieved if the current operation
            // index matches the path length, as the while loop will not run.
            if (currentOperationIndex == path.Length)
                break;

            var diagonal = diagonals[diagonalIndex];

            int xOperationIndex = -1, yOperationIndex = -1;

            while (currentOperationIndex < path.Length)
            {
                var operation = path[currentOperationIndex];

                // Skip this item if the payload already has the move flag.
                // If an item has already been processed, what was previously an encoded X coordinate will now be an encoded Y
                // coordinate and vice versa. If these new values match a non-processed value, this may select the wrong indexes.
                if (operation != 0 && (operation & DiffOperation.Move) == 0)
                {
                    // Nested loop search not required since we're querying both X and Y. With this approach, no matter
                    // which coordinate we find first, it is guaranteed that the next one will be after it in the path.

                    if (xOperationIndex == -1 && (operation & DiffOperation.Remove) != 0
                        && operation >> DiffOperation.Offset == diagonal.GlobalIndex / destinationArray.Length)
                    {
                        xOperationIndex = currentOperationIndex;
                    }
                    else if (yOperationIndex == -1 && (operation & DiffOperation.Insert) != 0
                             && operation >> DiffOperation.Offset == diagonal.GlobalIndex % destinationArray.Length)
                    {
                        yOperationIndex = currentOperationIndex;
                    }
                }

                if (xOperationIndex != -1 && yOperationIndex != -1)
                    break;

                currentOperationIndex++;
            }

            // Both values are required to process a move operation.
            if (xOperationIndex == -1 || yOperationIndex == -1)
                continue;

            var x = path[xOperationIndex] >> DiffOperation.Offset;
            var y = path[yOperationIndex] >> DiffOperation.Offset;

            // Append additional flags to payload.
            // Moves require the X/Y coordinates to be swapped, but the flags should stay the same.
            if (diffCallback.AreContentsTheSame(sourceArray[x], destinationArray[y]))
            {
                path[xOperationIndex] = (y << DiffOperation.Offset) | DiffOperation.Remove | DiffOperation.Move;
                path[yOperationIndex] = (x << DiffOperation.Offset) | DiffOperation.Insert | DiffOperation.Move;
            }
            else
            {
                path[xOperationIndex] = (y << DiffOperation.Offset) | DiffOperation.Remove | DiffOperation.Move | DiffOperation.Update;
                path[yOperationIndex] = (x << DiffOperation.Offset) | DiffOperation.Insert | DiffOperation.Move | DiffOperation.Update;
            }
        }

        return new DiffResult<TSource, TDestination>(diffCallback, sourceArray, destinationArray, path, diagonals.Count);
    }
    #endregion
}