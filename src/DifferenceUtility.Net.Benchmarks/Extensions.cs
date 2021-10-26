using System;
using System.Collections.Generic;
using System.Linq;

namespace DifferenceUtility.Net.Benchmarks
{
    public static class Extensions
    {
        #region Fields
        private static readonly Random _random = new();
        #endregion

        #region Public Methods
        /// <summary>
        /// Inserts the items from <paramref name="data" /> at random into the list.
        /// </summary>
        public static IList<T> InsertRandom<T>(this IList<T> list, IEnumerable<T> data)
        {
            var dataList = data.ToList();

            var dataListIndex = 0;
            var listIndex = 0;

            while (dataListIndex != dataList.Count)
            {
                if (_random.NextDouble() > 0.5)
                {
                    list.Insert(listIndex, dataList[dataListIndex]);

                    dataListIndex++;
                }

                listIndex = listIndex == list.Count - 1 ? 0 : listIndex += 1;
            }

            return list;
        }

        /// <summary>
        /// Removes a random selection of items from the list.
        /// </summary>
        public static IList<T> RemoveRandom<T>(this IList<T> list, int count = 1)
        {
            var index = 0;
            var removed = 0;

            while (removed != count)
            {
                if (_random.NextDouble() > 0.5)
                {
                    list.RemoveAt(index);

                    removed++;
                }

                index = index == count ? 0 : index += 1;
            }

            return list;
        }

        /// <summary>
        /// Suffles the list.
        /// </summary>
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;

            while (n > 1)
            {
                n--;
                var k = _random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
        #endregion
    }
}
