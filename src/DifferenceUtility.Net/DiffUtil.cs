using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.Instructions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DifferenceUtility.Net
{
    public static class DiffUtil
    {
        #region Public Methods
        /// <summary>
        /// Calculates the difference between <paramref name="oldCollection" /> and <paramref name="newCollection" />.
        /// </summary>
        /// <param name="diffCallback">A callback for calculating the difference between the provided collections.</param>
        /// <returns>A <see cref="DiffResult" /> with configured instructions.</returns>
        public static DiffResult CalculateDiff(IEnumerable oldCollection, IEnumerable newCollection, IDiffCallback diffCallback)
        {
            // Convert the provided collections to local arrays so that this method isn't affected by external changes.
            var oldArray = oldCollection.Cast<object>().ToArray();
            var newArray = newCollection.Cast<object>().ToArray();

            var instructions = new List<IDiffInstruction>();

            // Step 1: calculate the items that need to be removed.
            // After this step, we can guarantee that the final collection size will be that of the new collection.
            instructions.AddRange(oldArray.Where(o => !newArray.Any(n => diffCallback.AreItemsTheSame(o, n))).Select(x => new RemoveDiffInstruction(x)));

            for (var i = 0; i < newArray.Length; i++)
            {
                var newItem = newArray[i];

                // Insert instruction required if item does not already exist.
                if (oldArray.SingleOrDefault(o => diffCallback.AreItemsTheSame(o, newItem)) is not object existingItem)
                {
                    instructions.Add(new InsertDiffInstruction(newItem, i, diffCallback));
                    continue;
                }

                // Update instruction required if contents of existing item differ from the new item.
                if (!diffCallback.AreContentsTheSame(existingItem, newItem))
                    instructions.Add(new UpdateDiffInstruction(existingItem, newItem, diffCallback));

                // Move required if existing item's index is different in old collection compared to new.
                if (Array.IndexOf(oldArray, existingItem) != i)
                    instructions.Add(new MoveDiffInstruction(existingItem, i));
            }

            // Order of instructions to be applied in: remove, insert, move, update.
            return new DiffResult(instructions.OrderByDescending(i => i is RemoveDiffInstruction)
                .ThenByDescending(x => x is InsertDiffInstruction)
                .ThenByDescending(x => x is MoveDiffInstruction)
                .ThenByDescending(x => x is UpdateDiffInstruction)
                .ToArray());
        }

        /// <summary>
        /// Calculates the difference between <paramref name="oldCollection" /> and <paramref name="newCollection" />.
        /// </summary>
        /// <param name="diffCallback">A callback for calculating the difference between the provided collections.</param>
        /// <returns>A <see cref="DiffResult{T}" /> with configured instructions.</returns>
        public static DiffResult<T> CalculateDiff<T>(IEnumerable<T> oldCollection, IEnumerable<T> newCollection, IDiffCallback<T> diffCallback)
            where T : class
        {
            return new DiffResult<T>(CalculateDiffInstructions(oldCollection, newCollection, diffCallback));
        }

        /// <inheritdoc cref="CalculateDiff{T}(IEnumerable{T}, IEnumerable{T}, IDiffCallback{T})" />
        public static DiffResult<TOld> CalculateDiff<TOld, TNew>(IEnumerable<TOld> oldCollection, IEnumerable<TNew> newCollection, IDiffCallback<TOld, TNew> diffCallback)
            where TOld : class
            where TNew : class
        {
            return new DiffResult<TOld>(CalculateDiffInstructions(oldCollection, newCollection, diffCallback));
        }
        #endregion

        #region Private Methods
        private static IDiffInstruction[] CalculateDiffInstructions(IEnumerable oldCollection, IEnumerable newCollection, IDiffCallback diffCallback)
        {
            // Convert the provided collections to local arrays so that this method isn't affected by external changes.
            var oldArray = oldCollection.Cast<object>().ToArray();
            var newArray = newCollection.Cast<object>().ToArray();

            var instructions = new List<IDiffInstruction>();

            // First, calculate the items that need to be removed.
            // After this step, we can guarantee that the final collection size will be that of the new collection.
            instructions.AddRange(oldArray.Where(o => !newArray.Any(n => diffCallback.AreItemsTheSame(o, n))).Select(x => new RemoveDiffInstruction(x)));

            for (var i = 0; i < newArray.Length; i++)
            {
                var newItem = newArray[i];

                // Insert instruction required if item does not already exist.
                if (oldArray.SingleOrDefault(o => diffCallback.AreItemsTheSame(o, newItem)) is not object existingItem)
                {
                    instructions.Add(new InsertDiffInstruction(newItem, i, diffCallback));
                    continue;
                }

                // Update instruction required if contents of existing item differ from the new item.
                if (!diffCallback.AreContentsTheSame(existingItem, newItem))
                    instructions.Add(new UpdateDiffInstruction(existingItem, newItem, diffCallback));

                // Move required if existing item's index is different in old collection compared to new.
                if (Array.IndexOf(oldArray, existingItem) != i)
                    instructions.Add(new MoveDiffInstruction(existingItem, i));
            }

            // Order of instructions to be applied in: remove, insert, move, update.
            return instructions.OrderByDescending(i => i is RemoveDiffInstruction)
                .ThenByDescending(x => x is InsertDiffInstruction)
                .ThenByDescending(x => x is MoveDiffInstruction)
                .ThenByDescending(x => x is UpdateDiffInstruction)
                .ToArray();
        }
        #endregion
    }
}
