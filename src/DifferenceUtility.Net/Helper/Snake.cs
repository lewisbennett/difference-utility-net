using System;

namespace DifferenceUtility.Net.Helper
{
    internal readonly struct Snake
    {
        #region Properties
        /// <summary>
        /// End position in the old collection, exclusive.
        /// </summary>
        public int EndX { get; init; }
        
        /// <summary>
        /// End position in the new collection, exclusive.
        /// </summary>
        public int EndY { get; init; }
        
        /// <summary>
        /// <c>true</c> if this snake was created in the reverse search, <c>false</c> otherwise.
        /// </summary>
        public bool Reverse { get; init; }
        
        /// <summary>
        /// Position in the old collection.
        /// </summary>
        public int StartX { get; init; }
        
        /// <summary>
        /// Position in the new collection.
        /// </summary>
        public int StartY { get; init; }
        #endregion
        
        #region Public Methods
        public int DiagonalSize()
        {
            return Math.Min(EndX - StartX, EndY - StartY);
        }

        /// <summary>
        /// Extract the diagonal of the snake to make reasoning easier for the rest of the algorithm where we try to produce a path and also find moves.
        /// </summary>
        public Diagonal ToDiagonal()
        {
            // We are a pure diagonal.
            if (!HasAdditionOrRemoval())
                return new Diagonal(StartX, StartY, EndX - StartX);

            // Snake edge is at the end.
            if (Reverse)
                return new Diagonal(StartX, StartY, DiagonalSize());
            
            return IsAddition()
                ? new Diagonal(StartX, StartY + 1, DiagonalSize())      // Snake edge is at the beginning.
                : new Diagonal(StartX + 1, StartY, DiagonalSize());     
        }
        #endregion
        
        #region Private Methods
        private bool HasAdditionOrRemoval()
        {
            return EndY - StartY != EndX - StartX;
        }

        private bool IsAddition()
        {
            return EndY - StartY > EndX - StartX;
        }
        #endregion
    }
}
