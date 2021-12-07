using System.Collections.ObjectModel;
using DifferenceUtility.Net.Base;

namespace DifferenceUtility.Net.CollectionUpdateCallbacks
{
    public class ObservableCollectionUpdateCallback<TOld, TNew> : ICollectionUpdateCallback
    {
        #region Fields
        private readonly IDiffCallback<TOld, TNew> _diffCallback;
        private readonly TNew[] _newArray;
        private readonly ObservableCollection<TOld> _observableCollection;
        private readonly TOld[] _oldArray;
        #endregion
        
        #region Public Methods
        /// <inheritdoc />
        public void OnChanged(int position, int datasourcePosition, int count)
        {
            for (var i = 0; i < count; i++)
                _diffCallback.UpdateContents(_observableCollection[i + position], _newArray[datasourcePosition]);
        }

        /// <inheritdoc />
        public void OnInserted(int insertPosition, int itemPosition)
        {
            var item = _diffCallback.ConstructFinalItem(_newArray[itemPosition]);
            
            if (insertPosition > _observableCollection.Count - 1)
                _observableCollection.Add(item);
            
            else
                _observableCollection.Insert(insertPosition, item);
            
            // for (var i = 0; i < itemPosition; i++)
            // {
            //     var index = i + insertPosition;
            //     var item = _diffCallback.ConstructFinalItem(_newArray[index]);
            //
            //     if (index > _observableCollection.Count - 1)
            //         _observableCollection.Add(item);
            //
            //     else
            //         _observableCollection.Insert(index, item);
            // }
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
        public ObservableCollectionUpdateCallback(IDiffCallback<TOld, TNew> diffCallback, ObservableCollection<TOld> observableCollection, TOld[] oldArray, TNew[] newArray)
        {
            _diffCallback = diffCallback;
            _newArray = newArray;
            _observableCollection = observableCollection;
            _oldArray = oldArray;
        }
        #endregion
    }
}
