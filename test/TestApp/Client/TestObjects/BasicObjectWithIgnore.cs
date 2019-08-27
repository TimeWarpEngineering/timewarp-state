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

    public override bool Equals(object obj)
    {
      if (obj == null || obj.GetType() != typeof(BasicObjectWithIgnore))
        return false;

      var basicObject = (BasicObjectWithIgnore)obj;
      return Equals(basicObject);
    }

    public bool Equals(BasicObjectWithIgnore other)
    {
      if (other == null)
        return false;
      return other.BoolValue == BoolValue
          && other.ByteValue == ByteValue
          && other.IntValue == IntValue
          && other.LongValue == LongValue
          // && other.StringValue == StringValue
          ;
    }
  }
}
