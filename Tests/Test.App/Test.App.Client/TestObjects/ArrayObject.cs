// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable ArrangeMethodOrOperatorBody
// ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace AnyClone.Tests.TestObjects;

using System;
using System.Linq;

public class ArrayObject : IEquatable<ArrayObject>
{
  public byte[] ByteArray { get; set; }
  public double[] DoubleArray { get; set; }
  public int[] IntArray { get; set; }

  public override bool Equals(object aObject)
  {
    var basicObject = (ArrayObject)aObject;
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
