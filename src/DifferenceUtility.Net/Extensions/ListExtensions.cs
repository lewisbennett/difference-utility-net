using System.Collections.Generic;

namespace DifferenceUtility.Net.Extensions
{
    public static class ListExtensions
    {
        #region Public Methods
        public static void Add<T>(this List<T> list, int index, T item)
        {
            if (index > list.Count - 1)
                list.Add(item);
            
            else
                list.Insert(index, item);
        }
        #endregion
    }
}
