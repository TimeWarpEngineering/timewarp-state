// ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
namespace AnyClone.Tests.TestObjects;

public class DictionaryObject : IEquatable<DictionaryObject>
{
  public IDictionary<int, BasicObject> Collection { get; set; } = new Dictionary<int, BasicObject>();

  public override int GetHashCode() => base.GetHashCode();
  public override bool Equals(object aObject)
  {
    var basicObject = (DictionaryObject)aObject;
    return Equals(basicObject);
  }

  public bool Equals(DictionaryObject aOther)
  {
    var dictionaryComparer = new DictionaryComparer<int, BasicObject>();
    return dictionaryComparer.Equals(Collection, aOther.Collection);
  }
}
