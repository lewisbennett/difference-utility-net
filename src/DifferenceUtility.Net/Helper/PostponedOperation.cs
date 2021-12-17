namespace DifferenceUtility.Net.Helper;

internal readonly struct PostponedOperation
{
    #region Properties
    /// <summary>
    /// Gets the ID of the operation.
    /// </summary>
    public int OperationID { get; init; }
        
    /// <summary>
    /// Gets the postponed operation's X coordinate within the diff matrix (item position in source collection).
    /// </summary>
    public int X { get; init; }
        
    /// <summary>
    /// Gets the postponed operation's Y coordinate within the diff matrix (item position in destination collection).
    /// </summary>
    public int Y { get; init; }
    #endregion
    
    #region Static  Properties
    /// <summary>
    /// Gets an empty postponed operation.
    /// </summary>
    public static PostponedOperation Empty { get; } = new PostponedOperation
    {
        OperationID = -1,
        X = -1,
        Y = -1
    };
    #endregion
}