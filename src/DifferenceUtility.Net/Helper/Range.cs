namespace DifferenceUtility.Net.Helper
{
    /// <summary>
    /// <para>Represents a range in two lists that needs to be solved.</para>
    /// <para>This internal struct is used when running Myers' algorithm without recursion.</para>
    /// </summary>
    internal struct Range
    {
        #region Properties
        public int NewCollectionEnd { get; set; }
        
        public int NewCollectionStart { get; set; }
        
        public int OldCollectionEnd { get; set; }
        
        public int OldCollectionStart { get; set; }
        #endregion
    }
}
