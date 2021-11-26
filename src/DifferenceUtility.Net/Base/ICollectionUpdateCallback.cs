namespace DifferenceUtility.Net.Base
{
    /// <summary>
    /// An interface that can receive update operations that are applied to a collection.
    /// </summary>
    public interface ICollectionUpdateCallback
    {
        #region Methods
        /// <summary>
        /// Called when <paramref name="count" /> number of items are updated at the given position.
        /// </summary>
        /// <param name="position">The position of the item which has been updated.</param>
        /// <param name="datasourcePosition">The position of the item to use as a datasource to update the existing item.</param>
        /// <param name="count">The number of items which have changed.</param>
        void OnChanged(int position, int datasourcePosition, int count);
        
        /// <summary>
        /// Called when an item is inserted at the given <paramref name="insertPosition" />.
        /// </summary>
        /// <param name="insertPosition">The position to insert the new item at.</param>
        /// <param name="itemPosition">The position of the item in the new collection.</param>
        void OnInserted(int insertPosition, int itemPosition);

        /// <summary>
        /// Called when an item changes its position in the collection.
        /// </summary>
        /// <param name="fromPosition">The previous position of the item before the move.</param>
        /// <param name="toPosition">The new position of the item.</param>
        void OnMoved(int fromPosition, int toPosition);
        
        /// <summary>
        /// Called when <paramref name="count" /> number of items are removed from the given <paramref name="position" />.
        /// </summary>
        /// <param name="position">The position of the item which has been removed.</param>
        /// <param name="count">The number of items which have been removed.</param>
        void OnRemoved(int position, int count);
        #endregion
    }
}
