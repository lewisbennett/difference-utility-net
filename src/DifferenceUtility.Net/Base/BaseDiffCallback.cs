namespace DifferenceUtility.Net.Base;

public abstract class BaseDiffCallback<TSource, TDestination> : IDiffCallback<TSource, TDestination>
{
    #region Public Methods
    /// <inheritdoc />
    public virtual bool AreContentsTheSame(TSource sourceItem, TDestination destinationItem)
    {
        return true;
    }

    /// <inheritdoc />
    public abstract bool AreItemsTheSame(TSource sourceItem, TDestination destinationItem);

    /// <inheritdoc />
    public virtual TSource ConstructFinalItem(TDestination destinationItem)
    {
        if (destinationItem is TSource newOld)
            return newOld;

        return default;
    }

    /// <inheritdoc />
    public virtual void UpdateContents(TSource item, TDestination dataSource)
    {
    }
    #endregion
}