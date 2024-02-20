// ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace AnyClone.Tests.TestObjects;

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
