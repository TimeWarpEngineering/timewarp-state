using System;
using System.Collections.Generic;

namespace AnyClone.Tests.TestObjects
{
    public class DictionaryObject : IEquatable<DictionaryObject>
    {
        public IDictionary<int, BasicObject> Collection { get; set; } = new Dictionary<int, BasicObject>();

    public override int GetHashCode() => base.GetHashCode();
    public override bool Equals(object obj)
        {
            var basicObject = (DictionaryObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(DictionaryObject other)
        {
            var dictionaryComparer = new DictionaryComparer<int, BasicObject>();
            return dictionaryComparer.Equals(Collection, other.Collection);
       }
    }
}
