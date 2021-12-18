namespace DifferenceUtility.Net.Helper;

internal static class DiffOperation
{
    #region Constant Values
    /// <summary>
    /// Represents an insert operation.
    /// </summary>
    public const int Insert = 1;

    /// <summary>
    /// Represents a remove operation.
    /// </summary>
    public const int Remove = Insert << 1;
        
    /// <summary>
    /// Represents a move operation.
    /// </summary>
    public const int Move = Remove << 1;

    /// <summary>
    /// The number of bits to offset a coordinate value from its operation flags.
    /// </summary>
    public const int Offset = 4;
        
    /// <summary>
    /// Represents an update operation.
    /// </summary>
    public const int Update = Move << 1;
    #endregion
}