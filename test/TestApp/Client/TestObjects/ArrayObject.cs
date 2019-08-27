using System;
using System.Linq;

namespace AnyClone.Tests.TestObjects
{
    public class ArrayObject : IEquatable<ArrayObject>
    {
        public byte[] ByteArray { get; set; }
        public int[] IntArray { get; set; }
        public double[] DoubleArray { get; set; }

    public override int GetHashCode() => base.GetHashCode();

    public override bool Equals(object obj)
        {
            var basicObject = (ArrayObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(ArrayObject other)
        {
            return other.ByteArray.AsEnumerable().SequenceEqual(ByteArray)
                && other.IntArray.AsEnumerable().SequenceEqual(IntArray)
                && other.DoubleArray.AsEnumerable().SequenceEqual(DoubleArray)
                ;
        }
    }
}
