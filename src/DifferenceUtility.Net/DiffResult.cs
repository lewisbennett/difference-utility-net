using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.Schema;

namespace DifferenceUtility.Net
{
    public class DiffResult<TOld, TNew>
    {
        #region Fields
        private readonly IDiffCallback<TOld, TNew> _diffCallback;
        private readonly IEnumerable<(TOld, TNew, DiffStatus)> _diffInstructions;
        private bool _hasApplied;
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies the difference instructions to the <paramref name="collection" />. This method can only be run once and will silently fail if run again.
        /// </summary>
        public void Apply(ObservableCollection<TOld> collection)
        {
            if (_hasApplied)
                return;

            var i = 0;
            
            foreach (var (old, @new, diffStatus) in _diffInstructions)
            {
                switch (diffStatus)
                {
                    case DiffStatus.Deleted:
                        collection.Remove(old);
                        continue;

                    case DiffStatus.Equal:
                        
                        if (!_diffCallback.AreContentsTheSame(old, @new))
                            _diffCallback.UpdateContents(old, @new);
                        
                        break;

                    case DiffStatus.Inserted:

                        var item = _diffCallback.ConstructFinalItem(@new);
                        
                        // The Insert method will fail if we try to insert to the first or last position
                        // in the collection, so check whether we should be using the Add method instead.
                        if (i > collection.Count - 1)
                            collection.Add(item);
                        
                        else
                            collection.Insert(i, item);
                        
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                i++;
            }
            
            _hasApplied = true;
        }
        #endregion

        #region Constructors
        internal DiffResult(IDiffCallback<TOld, TNew> diffCallback, IEnumerable<(TOld, TNew, DiffStatus)> diffInstructions)
        {
            _diffCallback = diffCallback;
            _diffInstructions = diffInstructions;
        }
        #endregion
    }
}
