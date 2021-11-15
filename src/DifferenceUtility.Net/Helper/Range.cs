namespace DifferenceUtility.Net.Helper
{
    /// <summary>
    /// <para>Represents a range in two lists that needs to be solved.</para>
    /// <para>This internal struct is used when running Myers' algorithm without recursion.</para>
    /// <para>Ends are exclusive.</para>
    /// </summary>
    internal struct Range
    {
        #region Properties
        public int NewCollectionEnd { get; set; }
        
        public int NewCollectionStart { get; set; }
        
        public int OldCollectionEnd { get; set; }
        
        public int OldCollectionStart { get; set; }
        #endregion
        
        #region Public Methods
        public int GetOldCollectionSize()
        {
            return OldCollectionEnd - OldCollectionStart;
        }
    
        public int GetNewCollectionSize()
        {
            return NewCollectionEnd - NewCollectionStart;
        }
        #endregion
    }
}
