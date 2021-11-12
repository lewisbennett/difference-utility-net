namespace DifferenceUtility.Net.Helper
{
    /// <summary>
    /// <para>Represents an update that we skipped because it was a move.</para>
    /// <para>When an update is skipped, it is tracked as other updates are dispatched until the matching
    /// add/remove operation is found, at which point the tracked position is used to dispatch the update.</para>
    /// </summary>
    internal class PostponedUpdate
    {
        #region Properties
        /// <summary>
        /// Gets or sets the position with regards to the end of the list.
        /// </summary>
        public int CurrentPosition { get; set; }
        
        /// <summary>
        /// Gets the position in the collection that owns this item.
        /// </summary>
        public int PositionInOwnerCollection { get; }
        
        /// <summary>
        /// Gets whether this item is a removal.
        /// </summary>
        public bool Removal { get; }
        #endregion
        
        #region Constructors
        public PostponedUpdate(int currentPosition, int positionInOwnerCollection, bool removal)
        {
            CurrentPosition = currentPosition;
            PositionInOwnerCollection = positionInOwnerCollection;
            Removal = removal;
        }
        #endregion
    }
}
