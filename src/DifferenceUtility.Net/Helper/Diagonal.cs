namespace DifferenceUtility.Net.Helper;

internal readonly struct Diagonal
{
    #region Properties
    /// <summary>
    /// Gets the diagonal's score used for calculating the longest common subsequence. This is the number of diagonals that exist within range of this diagonal
    /// when traversing through the diff matrix from start to finish.
    /// </summary>
    public int Score { get; init; }
        
    /// <summary>
    /// Gets the diagonal's X coordinate within the diff matrix (item position in source collection).
    /// </summary>
    public int X { get; init; }
        
    /// <summary>
    /// Gets the diagonal's Y coordinate within the diff matrix (item position in destination collection).
    /// </summary>
    public int Y { get; init; }
    #endregion
}