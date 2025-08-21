// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
// ReSharper disable ArrangeMethodOrOperatorBody
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace AnyClone.Tests.TestObjects;

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

  public bool Equals(CollectionObject other)
  {
    return other.IntCollection.AsEnumerable().SequenceEqual(IntCollection)
        && other.ObjectCollection.AsEnumerable().SequenceEqual(ObjectCollection)
        ;
  }
}
