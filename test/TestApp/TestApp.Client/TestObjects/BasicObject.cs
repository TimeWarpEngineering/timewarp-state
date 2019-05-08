using System;

namespace AnyClone.Tests.TestObjects
{
    public class BasicObject : IEquatable<BasicObject>
    {
        private int _privateIntValue;
        public bool BoolValue { get; set; }
        public byte ByteValue { get; set; }
        public int IntValue { get; set; }
        public long LongValue { get; set; }
        public string StringValue { get; set; }

        public BasicObject() { }
        public BasicObject(int privateIntValue)
        {
            _privateIntValue = privateIntValue;
        }

    public override int GetHashCode() => base.GetHashCode();

    public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BasicObject))
                return false;

            var basicObject = (BasicObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(BasicObject other)
        {
            if (other == null)
                return false;
            return 
                other._privateIntValue == _privateIntValue
                && other.BoolValue == BoolValue
                && other.ByteValue == ByteValue
                && other.IntValue == IntValue
                && other.LongValue == LongValue
                && other.StringValue == StringValue
                ;
        }
    }
}
