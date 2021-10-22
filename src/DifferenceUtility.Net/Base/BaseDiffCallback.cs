namespace DifferenceUtility.Net.Base
{
    public abstract class BaseDiffCallback : IDiffCallback
    {
        #region Public Methods
        /// <inheritdoc />
        public abstract bool AreContentsTheSame(object oldItem, object newItem);

        /// <inheritdoc />
        public abstract bool AreItemsTheSame(object oldItem, object newItem);

        /// <inheritdoc />
        public virtual object ConstructFinalItem(object newItem)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual void UpdateContents(object item, object dataSource)
        {
        }
        #endregion
    }

    public abstract class BaseDiffCallback<T> : BaseDiffCallback, IDiffCallback<T>
        where T : class
    {
        #region Public Sealed Overritten Methods
        /// <inheritdoc />
        public sealed override bool AreContentsTheSame(object oldItem, object newItem)
        {
            return AreContentsTheSame((T)oldItem, (T)newItem);
        }

        /// <inheritdoc />
        public sealed override bool AreItemsTheSame(object oldItem, object newItem)
        {
            return AreItemsTheSame((T)oldItem, (T)newItem);
        }

        /// <inheritdoc />
        public sealed override object ConstructFinalItem(object newItem)
        {
            return ConstructFinalItem((T)newItem);
        }

        /// <inheritdoc />
        public sealed override void UpdateContents(object item, object dataSource)
        {
            base.UpdateContents(item, dataSource);

            UpdateContents((T)item, (T)dataSource);
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        public abstract bool AreContentsTheSame(T oldItem, T newItem);

        /// <inheritdoc />
        public abstract bool AreItemsTheSame(T oldItem, T newItem);

        /// <inheritdoc />
        public virtual T ConstructFinalItem(T newItem)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual void UpdateContents(T item, T dataSource)
        {
        }
        #endregion
    }

    public abstract class BaseDiffCallback<TOld, TNew> : BaseDiffCallback, IDiffCallback<TOld, TNew>
        where TOld : class
        where TNew : class
    {
        #region Public Sealed Overritten Methods
        /// <inheritdoc />
        public sealed override bool AreContentsTheSame(object oldItem, object newItem)
        {
            return AreContentsTheSame((TOld)oldItem, (TNew)newItem);
        }

        /// <inheritdoc />
        public sealed override bool AreItemsTheSame(object oldItem, object newItem)
        {
            return AreItemsTheSame((TOld)oldItem, (TNew)newItem);
        }

        /// <inheritdoc />
        public sealed override object ConstructFinalItem(object newItem)
        {
            return ConstructFinalItem((TNew)newItem);
        }

        /// <inheritdoc />
        public sealed override void UpdateContents(object item, object dataSource)
        {
            base.UpdateContents(item, dataSource);

            UpdateContents((TOld)item, (TNew)dataSource);
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        public abstract bool AreContentsTheSame(TOld oldItem, TNew newItem);

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
