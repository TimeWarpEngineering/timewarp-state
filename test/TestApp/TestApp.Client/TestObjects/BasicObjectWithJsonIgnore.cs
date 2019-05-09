//using Newtonsoft.Json;
//using System;

//namespace AnyClone.Tests.TestObjects
//{
//    public class BasicObjectWithJsonIgnore : IEquatable<BasicObjectWithJsonIgnore>
//    {
//        public bool BoolValue { get; set; }
//        public byte ByteValue { get; set; }
//        public int IntValue { get; set; }
//        public long LongValue { get; set; }
//        [JsonIgnore]
//        public string StringValue { get; set; }

//        public override int GetHashCode()
//        {
//            return base.GetHashCode();
//        }

//        public override bool Equals(object obj)
//        {
//            if (obj == null || obj.GetType() != typeof(BasicObjectWithJsonIgnore))
//                return false;
//            var basicObject = (BasicObjectWithJsonIgnore)obj;
//            return Equals(basicObject);
//        }

//        public bool Equals(BasicObjectWithJsonIgnore other)
//        {
//            if (other == null)
//                return false;
//            return other.BoolValue == BoolValue
//                && other.ByteValue == ByteValue
//                && other.IntValue == IntValue
//                && other.LongValue == LongValue
//                // && other.StringValue == StringValue
//                ;
//        }
//    }
//}
