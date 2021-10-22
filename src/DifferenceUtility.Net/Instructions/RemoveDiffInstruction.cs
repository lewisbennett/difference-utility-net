using DifferenceUtility.Net.Base;
using System.Collections.ObjectModel;

namespace DifferenceUtility.Net.Instructions
{
    public class RemoveDiffInstruction : IDiffInstruction
    {
        #region Fields
        private readonly object _item;
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies the instruction to the <paramref name="collection" />.
        /// </summary>
        public void Apply<T>(ObservableCollection<T> collection)
        {
            collection.Remove((T)_item);
        }
        #endregion

        #region Constructors
        public RemoveDiffInstruction(object item)
            : base()
        {
            _item = item;
        }
        #endregion
    }
}
