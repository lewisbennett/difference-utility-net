using System.Collections.ObjectModel;
using DifferenceUtility.Net.Base;

namespace DifferenceUtility.Net.Instructions
{
    public class UpdateDiffInstruction<TOld, TNew> : IDiffInstruction<TOld>
    {
        #region Fields
        private readonly TNew _dataSource;
        private readonly TOld _item;
        private readonly IDiffCallback<TOld, TNew> _diffCallback;
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies the instruction to the <paramref name="collection" />.
        /// </summary>
        public void Apply(ObservableCollection<TOld> collection)
        {
            _diffCallback.UpdateContents(_item, _dataSource);
        }
        #endregion

        #region Constructors
        public UpdateDiffInstruction(TOld item, TNew dataSource, IDiffCallback<TOld, TNew> diffCallback)
            : base()
        {
            _dataSource = dataSource;
            _diffCallback = diffCallback;
            _item = item;
        }
        #endregion
    }
}
