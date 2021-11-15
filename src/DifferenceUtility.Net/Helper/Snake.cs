namespace DifferenceUtility.Net.Helper
{
    internal struct Snake
    {
        #region Properties
        /// <summary>
        /// <c>true</c> if this is a removal from the original collection, followed by <see cref="Size" /> matches.
        /// <c>false</c> if this is an addition from the new collection followed by <see cref="Size" /> matches.
        /// </summary>
        public bool Removal { get; init; }
        
        /// <summary>
        /// <c>true</c> if the addition or removal is at the end of the snake.
        /// <c>false</c> if the addition or removal is at the start of the snake.
        /// </summary>
        public bool Reverse { get; init; }
        
        /// <summary>
        /// Gets the number of matches. Might be zero.
        /// </summary>
        public int Size { get; init; }
        
        /// <summary>
        /// Gets or sets the position in the old collection.
        /// </summary>
        public int X { get; set; }
        
        /// <summary>
        /// Gets or sets the position in the new collection.
        /// </summary>
        public int Y { get; set; }
        #endregion
    }
}
