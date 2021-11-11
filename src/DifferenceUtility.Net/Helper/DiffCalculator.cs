using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.Schema;

namespace DifferenceUtility.Net.Helper
{
    // Heavily based on NetDiff: https://github.com/skanmera/NetDiff/tree/master
    
    public class DiffCalculator<TOld, TNew>
    {
        #region Fields
        private readonly IDiffCallback<TOld, TNew> _diffCallback;
        private readonly Point _endpoint, _startPoint;
        private int[] _farthestPoints;
        private List<Node> _heads;
        private bool _isEnd;
        private readonly TNew[] _newCollection;
        private readonly TOld[] _oldCollection;
        private readonly int _offset;
        #endregion

        #region Public Methods
        public IEnumerable<Point> CalculatePath(out TOld[] oldArray, out TNew[] newArray)
        {
            oldArray = _oldCollection;
            newArray = _newCollection;
            
            if (!_oldCollection.Any())
                return Enumerable.Range(0, _newCollection.Length + 1).Select(i => new Point(0, i)).ToList();

            if (!_newCollection.Any())
                return Enumerable.Range(0, _oldCollection.Length + 1).Select(i => new Point(i, 0)).ToList();
        
            // Initialize.
            _farthestPoints = new int[_oldCollection.Length + _newCollection.Length + 1];
            
            _heads = new List<Node>
            {
                new(_startPoint)
            };

            Snake();

            while (CanMoveNext()) { }

            var waypoints = new List<Point>();

            var current = _heads.FirstOrDefault(h => h.Point.Equals(_endpoint));

            while (current is not null)
            {
                waypoints.Add(current.Point);

                current = current.Parent;
            }

            waypoints.Reverse();

            return waypoints;
        }
        #endregion
        
        #region Constructors
        public DiffCalculator([NotNull] IEnumerable<TOld> oldCollection, [NotNull] IEnumerable<TNew> newCollection, [NotNull] IDiffCallback<TOld, TNew> diffCallback)
        {
            _oldCollection = oldCollection as TOld[] ?? oldCollection?.ToArray() ?? throw new ArgumentNullException(nameof(oldCollection));
            _newCollection = newCollection as TNew[] ?? newCollection?.ToArray() ?? throw new ArgumentNullException(nameof(newCollection));
            
            _diffCallback = diffCallback ?? throw new ArgumentNullException(nameof(diffCallback));

            _startPoint = new Point(0, 0);
            _endpoint = new Point(_oldCollection.Length, _newCollection.Length);

            _offset = _newCollection.Length;
        }
        #endregion

        #region Private Methods
        private bool CanCreateHead(Direction direction, Point nextPoint)
        {
            if (!nextPoint.IsInRange(_startPoint, _endpoint))
                return false;

            if (direction != Direction.Diagonal)
                return UpdateFarthestPoint(nextPoint);

            return _diffCallback.AreItemsTheSame(_oldCollection[nextPoint.X - 1], _newCollection[nextPoint.Y - 1]) && UpdateFarthestPoint(nextPoint);
        }
        
        private bool CanMoveNext()
        {
            if (_isEnd)
                return false;

            UpdateHeads();
            
            return true;
        }

        private void Snake()
        {
            _heads = _heads.Select(head => Snake(head) ?? head).ToList();
        }

        private Node Snake(Node head)
        {
            Node newHead = null;

            while (true)
            {
                if (TryCreateHead(newHead ?? head, Direction.Diagonal, out var tmp))
                    newHead = tmp;
                
                else
                    break;
            }

            return newHead;
        }

        private bool TryCreateHead(Node head, Direction direction, out Node newHead)
        {
            newHead = null;

            var newPoint = head.Point.GetNextPoint(direction);

            if (!CanCreateHead(direction, newPoint))
                return false;

            newHead = new Node(newPoint)
            {
                Parent = head
            };

            _isEnd |= newHead.Point.Equals(_endpoint);

            return true;
        }

        private bool UpdateFarthestPoint(Point point)
        {
            var k = point.X - point.Y;
            var y = _farthestPoints[k + _offset];

            if (point.Y <= y)
                return false;

            _farthestPoints[k + _offset] = point.Y;

            return true;
        }
        
        private void UpdateHeads()
        {
            var updated = new List<Node>();

            foreach (var head in _heads)
            {
                if (TryCreateHead(head, Direction.Right, out var rightHead))
                    updated.Add(rightHead);
                
                if (TryCreateHead(head, Direction.Bottom, out var bottomHead))
                    updated.Add(bottomHead);
            }

            _heads = updated;

            Snake();
        }
        #endregion
    }
}
