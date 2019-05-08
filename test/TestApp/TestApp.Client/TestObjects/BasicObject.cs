namespace AnyClone.Tests.TestObjects
{
  using System;

  public class BasicObject : IEquatable<BasicObject>
  {
#pragma warning disable IDE0044 // Add readonly modifier
    private int _privateIntValue;
#pragma warning restore IDE0044 // Add readonly modifier
    public bool BoolValue { get; set; }
    public byte ByteValue { get; set; }
    public int IntValue { get; set; }
    public long LongValue { get; set; }
    public string StringValue { get; set; }

    public BasicObject() { }
    public BasicObject(int aPrivateIntValue)
    {
      _privateIntValue = aPrivateIntValue;
    }

    public override int GetHashCode() => base.GetHashCode();

    public override bool Equals(object aObject)
    {
      if (aObject == null || aObject.GetType() != typeof(BasicObject))
      {
        return false;
      }

      var basicObject = (BasicObject)aObject;
      return Equals(basicObject);
    }

    public bool Equals(BasicObject aBasicObject)
    {
      if (aBasicObject == null) return false;
      return
          aBasicObject._privateIntValue == _privateIntValue
          && aBasicObject.BoolValue == BoolValue
          && aBasicObject.ByteValue == ByteValue
          && aBasicObject.IntValue == IntValue
          && aBasicObject.LongValue == LongValue
          && aBasicObject.StringValue == StringValue
          ;
    }
  }
}
