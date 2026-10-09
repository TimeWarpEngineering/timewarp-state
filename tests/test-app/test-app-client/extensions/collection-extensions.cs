#region Purpose
// EnumerableEqual helper the clone tests use to compare multi-dimensional int arrays.
#endregion

#region Design
// Ported with the AnyClone test suite, so it keeps the AnyClone.Tests.Extensions namespace. Flattens both sequences
// into List<int> and compares them. A length mismatch is not equal, including when the shared prefix matches. Any
// exception (for example a non-int element, or an enumerator that cannot Reset) is reported as not equal.
#endregion

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
      bool lengthsDiffer = false;
      while (true)
      {
        bool firstMoved = enumerator.MoveNext();
        bool secondMoved = secondEnumerator.MoveNext();
        if (firstMoved != secondMoved)
        {
          lengthsDiffer = true;
          break;
        }

        if (!firstMoved)
          break;

        linearList.Add((int)enumerator.Current);
        otherLinearList.Add((int)secondEnumerator.Current);
      }

      enumerator.Reset();
      secondEnumerator.Reset();

      return !lengthsDiffer && linearList.SequenceEqual(otherLinearList);
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
