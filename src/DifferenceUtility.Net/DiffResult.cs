using System.Collections.Generic;
using System.Collections.ObjectModel;
using DifferenceUtility.Net.Base;

namespace DifferenceUtility.Net
{
    public class DiffResult<T>
    {
        #region Fields
        private readonly IEnumerable<IDiffInstruction<T>> _diffInstructions;
        private bool _hasApplied;
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies the difference instructions to the <paramref name="collection" />. This method can only be run once and will silently fail if run again.
        /// </summary>
        public void Apply(ObservableCollection<T> collection)
        {
            if (_hasApplied)
                return;

            foreach (var diffInstruction in _diffInstructions)
                diffInstruction.Apply(collection);

            _hasApplied = true;
        }
        #endregion

        #region Constructors
        public DiffResult(IEnumerable<IDiffInstruction<T>> diffInstructions)
        {
            _diffInstructions = diffInstructions;
        }
        #endregion
    }
}
