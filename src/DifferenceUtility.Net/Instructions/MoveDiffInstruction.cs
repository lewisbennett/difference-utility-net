using System.Collections.ObjectModel;
using DifferenceUtility.Net.Base;

namespace DifferenceUtility.Net.Instructions
{
    public class MoveDiffInstruction<TOld> : IDiffInstruction<TOld>
    {
        #region Fields
        private readonly TOld _item;
        private readonly int _moveIndex;
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies the instruction to the <paramref name="collection" />.
        /// </summary>
        public void Apply(ObservableCollection<TOld> collection)
        {
            collection.Move(collection.IndexOf(_item), _moveIndex);
        }
        #endregion

        #region Constructors
        public MoveDiffInstruction(TOld item, int moveIndex)
        {
            _item = item;
            _moveIndex = moveIndex;
        }
        #endregion
    }
}
