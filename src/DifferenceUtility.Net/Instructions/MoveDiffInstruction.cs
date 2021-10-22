using DifferenceUtility.Net.Base;
using System.Collections.ObjectModel;

namespace DifferenceUtility.Net.Instructions
{
    public class MoveDiffInstruction : IDiffInstruction
    {
        #region Fields
        private readonly object _item;
        private readonly int _moveIndex;
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies the instruction to the <paramref name="collection" />.
        /// </summary>
        public void Apply<T>(ObservableCollection<T> collection)
        {
            collection.Move(collection.IndexOf((T)_item), _moveIndex);
        }
        #endregion

        #region Constructors
        public MoveDiffInstruction(object item, int moveIndex)
            : base()
        {
            _item = item;
            _moveIndex = moveIndex;
        }
        #endregion
    }
}
