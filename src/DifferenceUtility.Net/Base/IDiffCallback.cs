namespace DifferenceUtility.Net.Base;

/// <summary>
///     An interface that allows the utility to interact with data types.
/// </summary>
public interface IDiffCallback<TSource, in TDestination>
{
    #region Methods
    /// <summary>
    ///     Gets whether there are any content differences between two items.
    ///     Will only be called if <see cref="AreItemsTheSame(TSource, TDestination)" /> returns <c>true</c>.
    /// </summary>
    bool AreContentsTheSame(TSource sourceItem, TDestination destinationItem);

    /// <summary>
    ///     Gets whether two objects represent the same data.
    /// </summary>
    bool AreItemsTheSame(TSource sourceItem, TDestination destinationItem);

    /// <summary>
    ///     Use this method to construct the final model representation of <paramref name="destinationItem" />.
    ///     Will only be called if <see cref="AreItemsTheSame(TSource, TDestination)" /> returns <c>false</c>.
    /// </summary>
    TSource ConstructFinalItem(TDestination destinationItem);

    /// <summary>
    ///     Updates the contents of the <paramref name="item" /> using the data provided by the <paramref name="dataSource" />.
    /// </summary>
    void UpdateContents(TSource item, TDestination dataSource);
    #endregion
}