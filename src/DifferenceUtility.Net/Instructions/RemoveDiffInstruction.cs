using System.Collections.ObjectModel;
using DifferenceUtility.Net.Base;

namespace DifferenceUtility.Net.Instructions
{
    public class RemoveDiffInstruction<TOld> : IDiffInstruction<TOld>
    {
        #region Fields
        private readonly TOld _item;
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies the instruction to the <paramref name="collection" />.
        /// </summary>
        public void Apply(ObservableCollection<TOld> collection)
        {
            collection.Remove(_item);
        }
        #endregion

        #region Constructors
        public RemoveDiffInstruction(TOld item)
            : base()
        {
            _item = item;
        }
        #endregion
    }
}
