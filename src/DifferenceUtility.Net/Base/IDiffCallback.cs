using System.Collections.ObjectModel;

namespace DifferenceUtility.Net.Base
{
    public interface IDiffCallback<TOld, in TNew>
    {
        #region Methods
        /// <summary>
        /// Gets whether there are any content differences between two items.
        /// 
        /// Will only be called if <see cref="AreItemsTheSame(TOld, TNew)" /> returns <c>true</c>.
        /// </summary>
        bool AreContentsTheSame(TOld oldItem, TNew newItem);

        /// <summary>
        /// Gets whether two objects represent the same data.
        /// </summary>
        bool AreItemsTheSame(TOld oldItem, TNew newItem);

        /// <summary>
        /// Use this method to construct the final model representation of <paramref name="newItem" />.
        /// 
        /// If no new model is required, return <c>null</c> to send <paramref name="newItem" /> to the final collection once <see cref="DiffResult{T}.Apply{T}(ObservableCollection{T})" /> is called.
        /// 
        /// Will only be called if <see cref="AreItemsTheSame(TOld, TNew)" /> returns <c>false</c>.
        /// </summary>
        TOld ConstructFinalItem(TNew newItem);

        /// <summary>
        /// Updates the contents of the <paramref name="item" /> using the data provided by the <paramref name="dataSource" />.
        /// </summary>
        void UpdateContents(TOld item, TNew dataSource);
        #endregion
    }
}
