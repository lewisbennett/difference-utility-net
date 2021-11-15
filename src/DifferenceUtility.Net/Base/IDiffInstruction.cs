using System.Collections.ObjectModel;

namespace DifferenceUtility.Net.Base
{
    public interface IDiffInstruction<T>
    {
        #region Methods
        /// <summary>
        /// Applies the instruction to the <paramref name="collection" />.
        /// </summary>
        void Apply(ObservableCollection<T> collection);
        #endregion
    }
}
