using System.Collections.ObjectModel;
using DifferenceUtility.Net.Base;

namespace DifferenceUtility.Net.Instructions
{
    public class InsertDiffInstruction<TOld, TNew> : IDiffInstruction<TOld>
    {
        #region Fields
        private readonly IDiffCallback<TOld, TNew> _diffCallback;
        private readonly int _insertIndex;
        private readonly TNew _item;
        #endregion

        #region Public Methods
        /// <inheritdoc />
        public void Apply(ObservableCollection<TOld> collection)
        {
            var item = _diffCallback.ConstructFinalItem(_item);

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
        public InsertDiffInstruction(TNew item, int insertIndex, IDiffCallback<TOld, TNew> diffCallback)
        {
            _diffCallback = diffCallback;
            _insertIndex = insertIndex;
            _item = item;
        }
        #endregion
    }
}
