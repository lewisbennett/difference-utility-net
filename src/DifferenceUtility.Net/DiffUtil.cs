using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.Instructions;

namespace DifferenceUtility.Net
{
    public static class DiffUtil
    {
        #region Public Methods
        /// <summary>
        /// Calculates the difference between <paramref name="oldCollection" /> and <paramref name="newCollection" />.
        /// </summary>
        /// <param name="diffCallback">A callback for calculating the difference between the provided collections.</param>
        /// <returns>A <see cref="DiffResult{T}" /> with configured instructions.</returns>
        public static DiffResult<TOld> CalculateDiff<TOld, TNew>(IEnumerable<TOld> oldCollection, IEnumerable<TNew> newCollection, IDiffCallback<TOld, TNew> diffCallback)
        {
            return new DiffResult<TOld>(CalculateDiffInstructions(oldCollection, newCollection, diffCallback));
        }
        #endregion

        #region Private Methods
        private static IEnumerable<IDiffInstruction<TOld>> CalculateDiffInstructions<TOld, TNew>(IEnumerable<TOld> oldCollection, IEnumerable<TNew> newCollection, IDiffCallback<TOld, TNew> diffCallback)
        {
            var oldArray = oldCollection as TOld[] ?? oldCollection?.ToArray();

            if (oldArray is null || !oldArray.Any())
                return Enumerable.Empty<IDiffInstruction<TOld>>();

            var newArray = newCollection as TNew[] ?? newCollection?.ToArray();

            if (newArray is null || !newArray.Any())
                return Enumerable.Empty<IDiffInstruction<TOld>>();

            var instructions = new List<IDiffInstruction<TOld>>();
            
            // First, calculate the items that need to be removed.
            // After this step, we can guarantee that the final collection size will be that of the new collection.
            instructions.AddRange(oldArray.Where(o => !newArray.Any(n => diffCallback.AreItemsTheSame(o, n))).Select(x => new RemoveDiffInstruction<TOld>(x)));
            
            for (var i = 0; i < newArray.Length; i++)
            {
                var newItem = newArray[i];
                
                // Insert instruction required if item does not already exist.
                if (oldArray.SingleOrDefault(o => diffCallback.AreItemsTheSame(o, newItem)) is not { } existingItem)
                {
                    
                    instructions.Add(new InsertDiffInstruction<TOld, TNew>(newItem, i, diffCallback));
                    continue;
                }

                // Update instruction required if contents of existing item differ from the new item.
                if (!diffCallback.AreContentsTheSame(existingItem, newItem))
                    instructions.Add(new UpdateDiffInstruction<TOld, TNew>(existingItem, newItem, diffCallback));
                
                // Move required if existing item's index is different in old collection compared to new.
                if (Array.IndexOf(oldArray, existingItem) != i)
                    instructions.Add(new MoveDiffInstruction<TOld>(existingItem, i));
            }
            
            // Order of instructions to be applied in: remove, insert, move, update.
            return instructions.OrderByDescending(i => i is RemoveDiffInstruction<TOld>)
                .ThenByDescending(x => x is InsertDiffInstruction<TOld, TNew>)
                .ThenByDescending(x => x is MoveDiffInstruction<TOld>)
                .ThenByDescending(x => x is UpdateDiffInstruction<TOld, TNew>)
                .ToArray();
        }
        #endregion
    }
}
