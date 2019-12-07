namespace AnyClone.Tests.TestObjects
{
  using AnyClone.Tests.Extensions;

  public class MultiDimensional2dArrayObject
  {
    public int[,] Int2DArray { get; set; } = new int[4, 2];

    public override int GetHashCode() => base.GetHashCode();

    public override bool Equals(object aObj)
    {
      var basicObject = (MultiDimensional2dArrayObject)aObj;
      return Equals(basicObject);
    }

    public bool Equals(MultiDimensional2dArrayObject aOther) => Int2DArray.EnumerableEqual(aOther.Int2DArray);
  }
}
