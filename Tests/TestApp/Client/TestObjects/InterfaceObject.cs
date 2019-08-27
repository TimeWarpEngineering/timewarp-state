using System;
using System.Collections.Generic;

namespace AnyClone.Tests.TestObjects
{
  public class InterfaceObject : ITestInterface, IEquatable<InterfaceObject>
  {
    public bool BoolValue { get; set; }
    public int IntValue { get; set; }
    public IDictionary<int, BasicObject> DictionaryValue { get; set; } = new Dictionary<int, BasicObject>();

    public override int GetHashCode() => base.GetHashCode();
    public override bool Equals(object obj)
    {
      var basicObject = (InterfaceObject)obj;
      return Equals(basicObject);
    }

    public bool Equals(InterfaceObject other)
    {
      var collectionComparer = new DictionaryComparer<int, BasicObject>();
      bool dictionaryIsEqual = collectionComparer.Equals(DictionaryValue, other.DictionaryValue);

      return dictionaryIsEqual && BoolValue == other.BoolValue && IntValue == other.IntValue;
    }
  }
}
