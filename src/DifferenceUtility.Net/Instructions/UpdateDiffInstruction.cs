using DifferenceUtility.Net.Base;
using System.Collections.ObjectModel;

namespace DifferenceUtility.Net.Instructions
{
    public class UpdateDiffInstruction : IDiffInstruction
    {
        #region Fields
        private readonly object _dataSource, _item;
        private readonly IDiffCallback _diffCallback;
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies the instruction to the <paramref name="collection" />.
        /// </summary>
        public void Apply<T>(ObservableCollection<T> collection)
        {
            _diffCallback.UpdateContents(_item, _dataSource);
        }
        #endregion

        #region Constructors
        public UpdateDiffInstruction(object item, object dataSource, IDiffCallback diffCallback)
            : base()
        {
            _dataSource = dataSource;
            _diffCallback = diffCallback;
            _item = item;
        }
        #endregion
    }
}
