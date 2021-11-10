namespace DifferenceUtility.Net.Base
{
    public abstract class BaseDiffCallback<TOld, TNew> : IDiffCallback<TOld, TNew>
        where TOld : class
        where TNew : class
    {
        #region Public Methods
        /// <inheritdoc />
        public virtual bool AreContentsTheSame(TOld oldItem, TNew newItem)
        {
            return true;
        }

        /// <inheritdoc />
        public abstract bool AreItemsTheSame(TOld oldItem, TNew newItem);

        /// <inheritdoc />
        public virtual TOld ConstructFinalItem(TNew newItem)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual void UpdateContents(TOld item, TNew dataSource)
        {
        }
        #endregion
    }
}
