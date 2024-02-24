// ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
namespace AnyClone.Tests.TestObjects;

public class MultiDimensional2dArrayObject
{
  public int[,] Int2DArray { get; set; } = new int[4, 2];

  public override int GetHashCode() => base.GetHashCode();

  public override bool Equals(object aObject)
  {
    var basicObject = (MultiDimensional2dArrayObject)aObject;
    return Equals(basicObject);
  }

  public bool Equals(MultiDimensional2dArrayObject aOther) => Int2DArray.EnumerableEqual(aOther.Int2DArray);
}
