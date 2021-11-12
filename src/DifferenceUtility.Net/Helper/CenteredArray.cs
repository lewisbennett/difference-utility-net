namespace DifferenceUtility.Net.Helper
{
    /// <summary>
    /// Array wrapper with negative index support.
    /// We use this array instead of a regular array so that the algorithm is easier to read without
    /// too many offsets when accessing the "k" array in the algorithm.
    /// </summary>
    internal class CenteredArray
    {
        #region Fields
        private readonly int _centerOffset;
        #endregion
        
        #region Properties
        public int[] Data { get; }
        #endregion
        
        #region Public Methods
        public int Get(int index)
        {
            return Data[index + _centerOffset];
        }

        public void Set(int index, int value)
        {
            Data[index + _centerOffset] = value;
        }
        #endregion
        
        #region Constructors
        public CenteredArray(int size)
        {
            Data = new int[size];
            
            _centerOffset = Data.Length / 2;
        }
        #endregion
    }
}
