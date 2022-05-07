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
    /// <param name="sourceItem">The item from the source collection.</param>
    /// <param name="destinationItem">The item from the destination collection.</param>
    /// <returns><c>true</c> if the contents of the objects are the same, otherwise <c>false</c>.</returns>
    bool AreContentsTheSame(TSource sourceItem, TDestination destinationItem);

    /// <summary>
    ///     Gets whether two objects represent the same data. Use this method to compare a unique trait of each object (such as
    ///     an ID).
    /// </summary>
    /// <param name="sourceItem">The item from the source collection.</param>
    /// <param name="destinationItem">The item from the destination collection.</param>
    /// <returns><c>true</c> if the two objects represent the same item, otherwise <c>false</c>.</returns>
    bool AreItemsTheSame(TSource sourceItem, TDestination destinationItem);

    /// <summary>
    ///     <para>
    ///         Use this method to construct the final 'model' representation of <paramref name="destinationItem" />.
    ///         Will only be called if <see cref="AreItemsTheSame(TSource, TDestination)" /> returns <c>false</c>.
    ///     </para>
    ///     <para>This method should not return <c>null</c>.</para>
    /// </summary>
    /// <param name="destinationItem">The item from the destination collection to create the final 'model' representation of.</param>
    /// <returns>An object that can be used within the source collection that represents <paramref name="destinationItem" />.</returns>
    TSource ConstructFinalItem(TDestination destinationItem);

    /// <summary>
    ///     <para>
    ///         Updates the contents of the <paramref name="item" /> using the data provided by the
    ///         <paramref name="dataSource" />.
    ///     </para>
    ///     <para>Will only be called if:</para>
    ///     <list type="bullet">
    ///         <item><see cref="AreItemsTheSame" /> returns <c>true</c>;</item>
    ///         <item><see cref="AreContentsTheSame" /> returns <c>false</c>;</item>
    ///         <item>The <paramref name="item" /> isn't involved in an insert or remove operation.</item>
    ///     </list>
    /// </summary>
    /// <param name="item">The item from the source collection.</param>
    /// <param name="dataSource">The item from the destination collection containing the new data.</param>
    void UpdateContents(TSource item, TDestination dataSource);
    #endregion
}