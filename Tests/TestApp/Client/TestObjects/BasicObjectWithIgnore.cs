namespace AnyClone.Tests.TestObjects
{
  using System;
  using System.Runtime.Serialization;

  public class BasicObjectWithIgnore : IEquatable<BasicObjectWithIgnore>
  {
    public bool BoolValue { get; set; }
    public byte ByteValue { get; set; }
    public int IntValue { get; set; }
    public long LongValue { get; set; }
    [IgnoreDataMember]
    public string StringValue { get; set; }

    public override int GetHashCode() => base.GetHashCode();

    public override bool Equals(object aObject)
    {
      if (aObject == null || aObject.GetType() != typeof(BasicObjectWithIgnore))
        return false;

      var basicObject = (BasicObjectWithIgnore)aObject;
      return Equals(basicObject);
    }

    public bool Equals(BasicObjectWithIgnore aOther)
    {
      if (aOther == null)
        return false;
      return aOther.BoolValue == BoolValue
          && aOther.ByteValue == ByteValue
          && aOther.IntValue == IntValue
          && aOther.LongValue == LongValue
          // && other.StringValue == StringValue
          ;
    }
  }
}