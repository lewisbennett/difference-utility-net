using System.Collections.ObjectModel;

namespace DifferenceUtility.Net.Base
{
    public interface IDiffInstruction
    {
        #region Methods
        /// <summary>
        /// Applies the instruction to the <paramref name="collection" />.
        /// </summary>
        void Apply<T>(ObservableCollection<T> collection);
        #endregion
    }
}
