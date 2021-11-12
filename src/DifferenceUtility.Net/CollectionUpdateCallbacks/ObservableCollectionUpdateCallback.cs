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
        public void OnInserted(int position, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var index = i + position;
                var item = _diffCallback.ConstructFinalItem(_newArray[index]);
                
                if (index > _observableCollection.Count - 1)
                    _observableCollection.Add(item);
                
                else
                    _observableCollection.Insert(index, item);
            }
        }

        /// <inheritdoc />
        public void OnMoved(int fromPosition, int toPosition)
        {
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
