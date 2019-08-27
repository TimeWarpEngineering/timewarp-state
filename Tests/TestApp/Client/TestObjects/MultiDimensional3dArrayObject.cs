using AnyClone.Tests.Extensions;

namespace AnyClone.Tests.TestObjects
{
  public class MultiDimensional3dArrayObject
  {
    public int[,,] Int3DArray { get; set; } = new int[2, 3, 3];

    public override int GetHashCode() => base.GetHashCode();

    public override bool Equals(object obj)
    {
      var basicObject = (MultiDimensional3dArrayObject)obj;
      return Equals(basicObject);
    }

    public bool Equals(MultiDimensional3dArrayObject other) => Int3DArray.EnumerableEqual(other.Int3DArray);
  }
}
