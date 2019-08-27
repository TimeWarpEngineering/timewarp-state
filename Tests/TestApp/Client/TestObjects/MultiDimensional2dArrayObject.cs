using AnyClone.Tests.Extensions;

namespace AnyClone.Tests.TestObjects
{
  public class MultiDimensional2dArrayObject
  {
    public int[,] Int2DArray { get; set; } = new int[4, 2];

    public override int GetHashCode() => base.GetHashCode();

    public override bool Equals(object obj)
    {
      var basicObject = (MultiDimensional2dArrayObject)obj;
      return Equals(basicObject);
    }

    public bool Equals(MultiDimensional2dArrayObject other) => Int2DArray.EnumerableEqual(other.Int2DArray);
  }
}
