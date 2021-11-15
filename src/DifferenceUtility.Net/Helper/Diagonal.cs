namespace DifferenceUtility.Net.Helper
{
    /// <summary>
    /// A diagonal is a match in the graph.
    /// Rather than snakes, we only record the diagonals in the path.
    /// </summary>
    internal readonly struct Diagonal
    {
        #region Properties
        public int Size { get; }

        /// <summary>
        /// Gets the X coordinate.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the Y coordinate.
        /// </summary>
        public int Y { get; }
        #endregion
        
        #region Public Methods
        public int GetEndX()
        {
            return X + Size;
        }

        public int GetEndY()
        {
            return Y + Size;
        }
        #endregion
        
        #region Constructors
        public Diagonal(int x, int y, int size)
        {
            Size = size;
            X = x;
            Y = y;
        }
        #endregion
    }
}
