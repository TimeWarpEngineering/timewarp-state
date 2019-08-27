using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnyClone.Tests.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Compare any enumerable to another enumerable
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool EnumerableEqual(this IEnumerable collection, IEnumerable second)
        {
            // optimal way to compare 2 multidimensional arrays is to flatten both of them then compare the entire set
            var linearList = new List<int>();
            var otherLinearList = new List<int>();
            var enumerator = collection.GetEnumerator();
            var secondEnumerator = second.GetEnumerator();
            try
            {
                while (enumerator.MoveNext() && secondEnumerator.MoveNext())
                {
                    linearList.Add((int)enumerator.Current);
                    otherLinearList.Add((int)secondEnumerator.Current);
                }
                enumerator.Reset();
                secondEnumerator.Reset();
                return linearList.SequenceEqual(otherLinearList);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
