using System;
using System.Collections.Generic;
using System.Linq;

namespace AnyClone.Tests.TestObjects
{
    public class CollectionObject : IEquatable<CollectionObject>
    {
        public ICollection<int> IntCollection { get; set; }
        public ICollection<BasicObject> ObjectCollection { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var basicObject = (CollectionObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(CollectionObject other)
        {
            return other.IntCollection.AsEnumerable().SequenceEqual(IntCollection)
                && other.ObjectCollection.AsEnumerable().SequenceEqual(ObjectCollection)
                ;
        }
    }
}
