namespace AnyClone.Tests.TestObjects
{
  using System;
  using System.Collections.Generic;

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

    public bool Equals(InterfaceObject aOther)
    {
      var collectionComparer = new DictionaryComparer<int, BasicObject>();
      bool dictionaryIsEqual = collectionComparer.Equals(DictionaryValue, aOther.DictionaryValue);

      return dictionaryIsEqual && BoolValue == aOther.BoolValue && IntValue == aOther.IntValue;
    }
  }
}