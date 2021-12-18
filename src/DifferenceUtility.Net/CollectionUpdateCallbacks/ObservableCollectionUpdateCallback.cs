﻿using System.Collections.ObjectModel;
using DifferenceUtility.Net.Base;

namespace DifferenceUtility.Net.CollectionUpdateCallbacks;

public class ObservableCollectionUpdateCallback<TSource, TDestination> : ICollectionUpdateCallback
{
    #region Fields
    private readonly TDestination[] _destinationArray;
    private readonly IDiffCallback<TSource, TDestination> _diffCallback;
    private readonly ObservableCollection<TSource> _observableCollection;
    #endregion
        
    #region Public Methods
    /// <inheritdoc />
    public void OnChanged(int position, int datasourcePosition, int count)
    {
        for (var i = 0; i < count; i++)
            _diffCallback.UpdateContents(_observableCollection[position + i], _destinationArray[datasourcePosition + i]);
    }

    /// <inheritdoc />
    public void OnInserted(int insertPosition, int itemPosition, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var index = i + insertPosition;
            var item = _diffCallback.ConstructFinalItem(_destinationArray[itemPosition + i]);
            
            if (index > _observableCollection.Count - 1)
                _observableCollection.Add(item);
            
            else
                _observableCollection.Insert(index, item);
        }
    }

    /// <inheritdoc />
    public void OnMoved(int fromPosition, int toPosition)
    {
        if (fromPosition != toPosition)
            _observableCollection.Move(fromPosition, toPosition);
    }
        
    /// <inheritdoc />
    public void OnRemoved(int position, int count)
    {
        for (var i = 0; i < count; i++)
            _observableCollection.RemoveAt(position);
    }
    #endregion
        
    #region Constructors
    public ObservableCollectionUpdateCallback(IDiffCallback<TSource, TDestination> diffCallback, ObservableCollection<TSource> observableCollection, TDestination[] destinationArray)
    {
        _destinationArray = destinationArray;
        _diffCallback = diffCallback;
        _observableCollection = observableCollection;
    }
    #endregion
}