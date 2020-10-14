namespace AnyClone.Tests.TestObjects
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class CollectionObject : IEquatable<CollectionObject>
  {
    public ICollection<int> IntCollection { get; set; }
    public ICollection<BasicObject> ObjectCollection { get; set; }

    public override int GetHashCode() => base.GetHashCode();

    public override bool Equals(object aObject)
    {
      var basicObject = (CollectionObject)aObject;
      return Equals(basicObject);
    }

    public bool Equals(CollectionObject aOther)
    {
      return aOther.IntCollection.AsEnumerable().SequenceEqual(IntCollection)
          && aOther.ObjectCollection.AsEnumerable().SequenceEqual(ObjectCollection)
          ;
    }
  }
}