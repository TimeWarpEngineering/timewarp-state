// ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
namespace AnyClone.Tests.TestObjects;

public class InterfaceObject : ITestInterface, IEquatable<InterfaceObject>
{
  public bool BoolValue { get; set; }
  public int IntValue { get; set; }
  public IDictionary<int, BasicObject> DictionaryValue { get; set; } = new Dictionary<int, BasicObject>();

  public override int GetHashCode() => base.GetHashCode();
  public override bool Equals(object aObject)
  {
    var basicObject = (InterfaceObject)aObject;
    return Equals(basicObject);
  }

  public bool Equals(InterfaceObject other)
  {
    var collectionComparer = new DictionaryComparer<int, BasicObject>();
    bool dictionaryIsEqual = collectionComparer.Equals(DictionaryValue, other.DictionaryValue);

    return dictionaryIsEqual && BoolValue == other.BoolValue && IntValue == other.IntValue;
  }
}
