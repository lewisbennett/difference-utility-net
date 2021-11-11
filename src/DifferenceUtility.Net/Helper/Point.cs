using System;
using DifferenceUtility.Net.Schema;

namespace DifferenceUtility.Net.Helper
{
    public readonly struct Point : IEquatable<Point>
    {
        #region Properties
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
        /// <inheritdoc />
        public bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Point other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        /// <summary>
        /// Gets the next point in a particular direction.
        /// </summary>
        public Point GetNextPoint(Direction direction)
        {
            return direction switch
            {
                Direction.Bottom => new Point(X, Y + 1),
                Direction.Diagonal => new Point(X + 1, Y + 1),
                Direction.Right => new Point(X + 1, Y),
                _ => throw new ArgumentException()
            };
        }

        /// <summary>
        /// Gets whether this point is within a range defined by two points.
        /// </summary>
        public bool IsInRange(Point startPoint, Point endpoint)
        {
            return X >= startPoint.X && Y >= startPoint.Y && X <= endpoint.X && Y <= endpoint.Y;
        }
        #endregion

        #region Constructors
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        #endregion
    }
}
