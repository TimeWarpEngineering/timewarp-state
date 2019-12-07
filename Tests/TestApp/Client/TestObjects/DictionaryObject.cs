namespace AnyClone.Tests.TestObjects
{
  using System;
  using System.Collections.Generic;

  public class DictionaryObject : IEquatable<DictionaryObject>
    {
        public IDictionary<int, BasicObject> Collection { get; set; } = new Dictionary<int, BasicObject>();

    public override int GetHashCode() => base.GetHashCode();
    public override bool Equals(object aObj)
        {
            var basicObject = (DictionaryObject)aObj;
            return Equals(basicObject);
        }

        public bool Equals(DictionaryObject aOther)
        {
            var dictionaryComparer = new DictionaryComparer<int, BasicObject>();
            return dictionaryComparer.Equals(Collection, aOther.Collection);
       }
    }
}
