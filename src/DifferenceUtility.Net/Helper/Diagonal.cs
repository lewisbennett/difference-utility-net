namespace DifferenceUtility.Net.Helper
{
    internal struct Diagonal
    {
        #region Properties
        /// <summary>
        /// Gets the diagonal's score used for calculating the longest common subsequence.
        /// </summary>
        public int Score { get; init; }
        
        /// <summary>
        /// Gets the diagonal's X coordinate (position in old collection).
        /// </summary>
        public int X { get; init; }
        
        /// <summary>
        /// Gets the diagonal's Y coordinate (position in new collection).
        /// </summary>
        public int Y { get; init; }
        #endregion
    }
}
