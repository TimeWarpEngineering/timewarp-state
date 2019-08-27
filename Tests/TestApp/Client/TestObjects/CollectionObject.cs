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
