// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
namespace AnyClone.Tests.TestObjects;

public class MultiDimensional3dArrayObject
{
  public int[,,] Int3DArray { get; set; } = new int[2, 3, 3];

  public override int GetHashCode() => base.GetHashCode();

  public override bool Equals(object aObject)
  {
    var basicObject = (MultiDimensional3dArrayObject)aObject;
    return Equals(basicObject);
  }

  public bool Equals(MultiDimensional3dArrayObject aOther) => Int3DArray.EnumerableEqual(aOther.Int3DArray);
}
