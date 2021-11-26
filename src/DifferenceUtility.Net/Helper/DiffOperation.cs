namespace DifferenceUtility.Net.Helper
{
    public static class DiffOperation
    {
        #region Constant Values
        /// <summary>
        /// Represents an insert operation.
        /// </summary>
        public const int Insert = NoOperation << 1;

        /// <summary>
        /// Represents a remove operation.
        /// </summary>
        public const int Remove = Insert << 1;
        
        public const int Mask = (1 << Offset) - 1;
        
        /// <summary>
        /// Represents a move operation.
        /// </summary>
        public const int Move = Remove << 1;

        /// <summary>
        /// Represents an operation that should not be ignored, but does not move.
        /// </summary>
        public const int NoOperation = 1;

        public const int Offset = 8;
        
        /// <summary>
        /// Represents an update operation.
        /// </summary>
        public const int Update = Move << 1;
        #endregion
    }
}
