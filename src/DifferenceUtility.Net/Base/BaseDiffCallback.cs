namespace DifferenceUtility.Net.Base;

/// <summary>
///     <para>Provides a base <see cref="IDiffCallback{TSource,TDestination}" /> implementation.</para>
///     <para>
///         The only required override is <see cref="AreItemsTheSame" />. Unless overridden, the remaining base method
///         implementations are as follows:
///     </para>
///     <para><see cref="AreContentsTheSame" /> will always return <c>true</c>.</para>
///     <para>
///         <see cref="ConstructFinalItem" /> will return the default for <typeparamref name="TSource" /> unless
///         <typeparamref name="TDestination" />
///         is the same type, in which case the object from the destination collection will be returned.
///     </para>
///     <para><see cref="UpdateContents" /> is an empty method and will take no action.</para>
/// </summary>
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