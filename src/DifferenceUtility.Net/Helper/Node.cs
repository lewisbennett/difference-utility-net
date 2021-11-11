namespace DifferenceUtility.Net.Helper
{
    public class Node
    {
        #region Properties
        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        public Node Parent { get; set; }
            
        /// <summary>
        /// Gets the point.
        /// </summary>
        public Point Point { get; }
        #endregion
            
        #region Constructors
        public Node(Point point)
        {
            Point = point;
        }
        #endregion
    }
}
