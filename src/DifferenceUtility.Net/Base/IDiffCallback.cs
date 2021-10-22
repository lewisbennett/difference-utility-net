using System.Collections.ObjectModel;

namespace DifferenceUtility.Net.Base
{
    public interface IDiffCallback
    {
        #region Methods
        /// <summary>
        /// Gets whether there are any content differences between two items.
        /// 
        /// Will only be called if <see cref="AreItemsTheSame(object, object)" /> returns <c>true</c>.
        /// </summary>
        bool AreContentsTheSame(object oldItem, object newItem);

        /// <summary>
        /// Gets whether two objects represent the same data.
        /// </summary>
        bool AreItemsTheSame(object oldItem, object newItem);

        /// <summary>
        /// Use this method to construct the final model representation of <paramref name="newItem" />.
        /// 
        /// If no new model is required, return <c>null</c> to send <paramref name="newItem" /> to the final collection once <see cref="DiffResult.Apply{T}(ObservableCollection{T})" /> is called.
        /// 
        /// Will only be called if <see cref="AreItemsTheSame(object, object)" /> returns <c>false</c>.
        /// </summary>
        object ConstructFinalItem(object newItem);

        /// <summary>
        /// Updates the contents of the <paramref name="item" /> using the data provided by the <paramref name="dataSource" />.
        /// </summary>
        void UpdateContents(object item, object dataSource);
        #endregion
    }

    public interface IDiffCallback<T> : IDiffCallback
        where T : class
    {
        #region Methods
        /// <summary>
        /// Gets whether there are any content differences between two items.
        /// 
        /// Will only be called if <see cref="AreItemsTheSame(T, T)" /> returns <c>true</c>.
        /// </summary>
        bool AreContentsTheSame(T oldItem, T newItem);

        /// <inheritdoc cref="IDiffCallback.AreItemsTheSame(object, object)" />.
        bool AreItemsTheSame(T oldItem, T newItem);

        /// <summary>
        /// Use this method to construct the final model representation of <paramref name="newItem" />.
        /// 
        /// If no new model is required, return <c>null</c> to send <paramref name="newItem" /> to the final collection once <see cref="DiffResult.Apply{T}(ObservableCollection{T})" /> is called.
        /// 
        /// Will only be called if <see cref="AreItemsTheSame(T, T)" /> returns <c>false</c>.
        /// </summary>
        T ConstructFinalItem(T newItem);

        /// <inheritdoc cref="IDiffCallback.UpdateContents(object, object)" />.
        void UpdateContents(T item, T dataSource);
        #endregion
    }

    public interface IDiffCallback<TOld, TNew> : IDiffCallback
        where TOld : class
        where TNew : class
    {
        #region Methods
        /// <summary>
        /// Gets whether there are any content differences between two items.
        /// 
        /// Will only be called if <see cref="AreItemsTheSame(TOld, TNew)" /> returns <c>true</c>.
        /// </summary>
        bool AreContentsTheSame(TOld oldItem, TNew newItem);

        /// <inheritdoc cref="IDiffCallback.AreItemsTheSame(object, object)" />.
        bool AreItemsTheSame(TOld oldItem, TNew newItem);

        /// <summary>
        /// Use this method to construct the final model representation of <paramref name="newItem" />.
        /// 
        /// If no new model is required, return <c>null</c> to send <paramref name="newItem" /> to the final collection once <see cref="DiffResult.Apply{T}(ObservableCollection{T})" /> is called.
        /// 
        /// Will only be called if <see cref="AreItemsTheSame(TOld, TNew)" /> returns <c>false</c>.
        /// </summary>
        TOld ConstructFinalItem(TNew newItem);

        /// <inheritdoc cref="IDiffCallback.UpdateContents(object, object)" />.
        void UpdateContents(TOld item, TNew dataSource);
        #endregion
    }
}
