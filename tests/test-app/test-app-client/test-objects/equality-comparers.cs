// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
namespace AnyClone.Tests.TestObjects;

public class DictionaryComparer<TKey, TValue> : IEqualityComparer<IDictionary<TKey, TValue>>
{
  private readonly IEqualityComparer<TValue> ValueComparer;

  public DictionaryComparer(IEqualityComparer<TValue> valueComparer = null)
  {
    this.ValueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
  }

  public bool Equals(IDictionary<TKey, TValue> dictionary1, IDictionary<TKey, TValue> dictionary2)
  {
    if (dictionary1.Count != dictionary2.Count) return false;
    if (dictionary1.Keys.Except(dictionary2.Keys).Any()) return false;
    // ReSharper disable once ConvertIfStatementToReturnStatement
    if (dictionary2.Keys.Except(dictionary1.Keys).Any()) return false;

    return dictionary1.All(pair => ValueComparer.Equals(pair.Value, dictionary2[pair.Key]));
  }

  public int GetHashCode(IDictionary<TKey, TValue> dictionary) => 0;
}
