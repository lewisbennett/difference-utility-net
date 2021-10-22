using DifferenceUtility.Net.Base;
using System.Collections.ObjectModel;

namespace DifferenceUtility.Net
{
    public class DiffResult
    {
        #region Fields
        private readonly IDiffInstruction[] _diffInstructions;
        private bool _hasApplied;
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies the difference instructions to the <paramref name="collection" />. This method can only be run once and will silently fail if run again.
        /// </summary>
        public void Apply<T>(ObservableCollection<T> collection)
        {
            if (_hasApplied)
                return;

            foreach (var diffInstruction in _diffInstructions)
                diffInstruction.Apply(collection);

            _hasApplied = true;
        }
        #endregion

        #region Constructors
        public DiffResult(IDiffInstruction[] diffInstructions)
        {
            _diffInstructions = diffInstructions;
        }
        #endregion
    }

    public class DiffResult<T>
        where T : class
    {
        #region Fields
        private readonly IDiffInstruction[] _diffInstructions;
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
        public DiffResult(IDiffInstruction[] diffInstructions)
        {
            _diffInstructions = diffInstructions;
        }
        #endregion
    }
}
