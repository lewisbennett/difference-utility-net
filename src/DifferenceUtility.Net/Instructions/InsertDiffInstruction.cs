using DifferenceUtility.Net.Base;
using System.Collections.ObjectModel;

namespace DifferenceUtility.Net.Instructions
{
    public class InsertDiffInstruction : IDiffInstruction
    {
        #region Fields
        private readonly IDiffCallback _diffCallback;
        private readonly int _insertIndex;
        private readonly object _item;
        #endregion

        #region Public Methods
        /// <inheritdoc />
        public void Apply<T>(ObservableCollection<T> collection)
        {
            var item = (T)(_diffCallback.ConstructFinalItem(_item) ?? _item);

            // The Insert method will fail if we try to insert to the last position in the collection,
            // so check whether we should be using the Add method instead.
            if (_insertIndex > collection.Count - 1)
            {
                collection.Add(item);
                return;
            }

            // Otherwise, insert.
            collection.Insert(_insertIndex, item);
        }
        #endregion

        #region Constructors
        public InsertDiffInstruction(object item, int insertIndex, IDiffCallback diffCallback)
            : base()
        {
            _diffCallback = diffCallback;
            _insertIndex = insertIndex;
            _item = item;
        }
        #endregion
    }
}
