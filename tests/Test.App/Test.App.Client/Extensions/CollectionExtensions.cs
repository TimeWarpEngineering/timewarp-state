namespace AnyClone.Tests.Extensions;

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
    IEnumerator enumerator = collection.GetEnumerator();
    IEnumerator secondEnumerator = second.GetEnumerator();
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
    finally
    {
      (enumerator as IDisposable)?.Dispose();
      (secondEnumerator as IDisposable)?.Dispose();
    }
  }
}
