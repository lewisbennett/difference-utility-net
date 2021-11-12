﻿using System;
using System.Diagnostics.CodeAnalysis;
using DifferenceUtility.Net.Base;

namespace DifferenceUtility.Net.CollectionUpdateCallbacks
{
    public class BatchingCollectionUpdateCallback : ICollectionUpdateCallback
    {
        #region Fields
        private int _lastEventCount = -1, _lastEventDataSourcePosition = -1, _lastEventPosition = -1, _lastEventType = TypeNone;
        private readonly ICollectionUpdateCallback _wrappedCallback;
        #endregion
        
        #region Public Methods
        /// <summary>
        /// BatchingCollectionUpdateCallback holds onto the last event to see if it can be merged with the next one.
        /// When stream of events finish, you should call this method to dispatch the last event.
        /// </summary>
        public void DispatchLastEvent()
        {
            switch (_lastEventType)
            {
                case TypeAdd:
                    _wrappedCallback.OnInserted(_lastEventPosition, _lastEventCount);
                    break;
                
                case TypeChange:
                    _wrappedCallback.OnChanged(_lastEventPosition, _lastEventDataSourcePosition, _lastEventCount);
                    break;
                
                case TypeRemove:
                    _wrappedCallback.OnRemoved(_lastEventPosition, _lastEventCount);
                    break;

                default:
                    return;
            }

            _lastEventDataSourcePosition = -1;
            _lastEventType = TypeNone;
        }
        
        /// <inheritdoc />
        public void OnChanged(int position, int datasourcePosition, int count)
        {
            if (_lastEventType == TypeChange &&
                !(position > _lastEventPosition + _lastEventCount || position + count < _lastEventPosition || _lastEventDataSourcePosition != datasourcePosition))
            {
                // Take potential overlap into account.
                var previousEnd = _lastEventPosition + _lastEventCount;

                _lastEventPosition = Math.Min(position, _lastEventPosition);
                _lastEventCount = Math.Max(previousEnd, position + count) - _lastEventPosition;

                return;
            }
            
            DispatchLastEvent();

            _lastEventCount = count;
            _lastEventDataSourcePosition = datasourcePosition;
            _lastEventPosition = position;
            _lastEventType = TypeChange;
        }
        
        /// <inheritdoc />
        public void OnInserted(int position, int count)
        {
            if (_lastEventType == TypeAdd && position >= _lastEventPosition && position <= _lastEventPosition + _lastEventCount)
            {
                _lastEventCount += count;
                _lastEventPosition = Math.Min(position, _lastEventPosition);

                return;
            }
            
            DispatchLastEvent();

            _lastEventCount = count;
            _lastEventPosition = position;
            _lastEventType = TypeAdd;
        }

        /// <inheritdoc />
        public void OnMoved(int fromPosition, int toPosition)
        {
            // Moves are not merged.
            DispatchLastEvent();
            
            _wrappedCallback.OnMoved(fromPosition, toPosition);
        }
        
        /// <inheritdoc />
        public void OnRemoved(int position, int count)
        {
            if (_lastEventType == TypeRemove && _lastEventPosition >= position && _lastEventPosition <= position + count)
            {
                _lastEventCount += count;
                _lastEventPosition = position;

                return;
            }
            
            DispatchLastEvent();

            _lastEventCount = count;
            _lastEventPosition = position;
            _lastEventType = TypeRemove;
        }
        #endregion
        
        #region Constructors
        public BatchingCollectionUpdateCallback([NotNull] ICollectionUpdateCallback callback)
        {
            _wrappedCallback = callback;
        }
        #endregion
        
        #region Private Constant Values
        private const int TypeNone = 0;
        private const int TypeAdd = 1;
        private const int TypeRemove = 2;
        private const int TypeChange = 3;
        #endregion
    }
}