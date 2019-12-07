namespace AnyClone.Tests.TestObjects
{
  using System;
  using System.Linq;

  public class ArrayObject : IEquatable<ArrayObject>
  {
    public byte[] ByteArray { get; set; }
    public double[] DoubleArray { get; set; }
    public int[] IntArray { get; set; }

    public override bool Equals(object obj)
    {
      var basicObject = (ArrayObject)obj;
      return Equals(basicObject);
    }

    public bool Equals(ArrayObject aOther)
    {
      return aOther.ByteArray.AsEnumerable().SequenceEqual(ByteArray)
          && aOther.IntArray.AsEnumerable().SequenceEqual(IntArray)
          && aOther.DoubleArray.AsEnumerable().SequenceEqual(DoubleArray)
          ;
    }

    public override int GetHashCode() => base.GetHashCode();
  }
}