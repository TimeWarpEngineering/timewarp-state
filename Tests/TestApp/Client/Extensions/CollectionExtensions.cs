namespace AnyClone.Tests.Extensions
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;

  public static class CollectionExtensions
  {
    /// <summary>
    /// Compare any enumerable to another enumerable
    /// </summary>
    /// <param name="aCollection"></param>
    /// <param name="aSecond"></param>
    /// <returns></returns>
    public static bool EnumerableEqual(this IEnumerable aCollection, IEnumerable aSecond)
    {
      // optimal way to compare 2 multidimensional arrays is to flatten both of them then compare the entire set
      var linearList = new List<int>();
      var otherLinearList = new List<int>();
      IEnumerator enumerator = aCollection.GetEnumerator();
      IEnumerator secondEnumerator = aSecond.GetEnumerator();
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