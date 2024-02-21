// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
namespace AnyClone.Tests.TestObjects;

public class DictionaryComparer<TKey, TValue> : IEqualityComparer<IDictionary<TKey, TValue>>
{
  private readonly IEqualityComparer<TValue> ValueComparer;

  public DictionaryComparer(IEqualityComparer<TValue> aValueComparer = null)
  {
    ValueComparer = aValueComparer ?? EqualityComparer<TValue>.Default;
  }

  public bool Equals(IDictionary<TKey, TValue> aDictionary1, IDictionary<TKey, TValue> aDictionary2)
  {
    if (aDictionary1.Count != aDictionary2.Count) return false;
    if (aDictionary1.Keys.Except(aDictionary2.Keys).Any()) return false;
    if (aDictionary2.Keys.Except(aDictionary1.Keys).Any()) return false;

    foreach (KeyValuePair<TKey, TValue> pair in aDictionary1)
    {
      if (!ValueComparer.Equals(pair.Value, aDictionary2[pair.Key])) return false;
    }

    return true;
  }

  public int GetHashCode(IDictionary<TKey, TValue> aDictionary) => 0;
}
