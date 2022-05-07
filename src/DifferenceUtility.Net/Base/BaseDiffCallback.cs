using System;

namespace DifferenceUtility.Net.Base;

/// <summary>
///     <para>Provides a base <see cref="IDiffCallback{TSource,TDestination}" /> implementation.</para>
///     <para>
///         The only required override is <see cref="AreItemsTheSame" />. Unless overridden, the remaining base method
///         implementations are as follows:
///     </para>
///     <list type="bullet">
///         <item>
///             <see cref="AreContentsTheSame" /> will always return <c>true</c>.
///         </item>
///         <item>
///             <see cref="ConstructFinalItem" /> will return the destination item if it is of type
///             <typeparamref name="TSource" />, otherwise
///             will throw a <see cref="NotImplementedException" />.
///         </item>
///         <item>
///             <see cref="UpdateContents" /> is an empty method and will take no action.
///         </item>
///     </list>
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

    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <param name="destinationItem">
    ///     <inheritdoc />
    /// </param>
    /// <returns>
    ///     <inheritdoc />
    /// </returns>
    /// <exception cref="NotImplementedException">
    ///     If <paramref name="destinationItem" /> is not of type
    ///     <typeparamref name="TSource" />.
    /// </exception>
    public virtual TSource ConstructFinalItem(TDestination destinationItem)
    {
        if (destinationItem is TSource item)
            return item;

        throw new NotImplementedException("Valid return type not implemented");
    }

    /// <inheritdoc />
    public virtual void UpdateContents(TSource item, TDestination dataSource)
    {
    }
    #endregion
}