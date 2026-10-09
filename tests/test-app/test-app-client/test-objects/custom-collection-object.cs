#region Purpose
// Clone test object: a Collection<T> subclass with extra properties and no parameterless constructor.
#endregion

#region Design
// Tests that the cloner handles a collection type that must be created through a constructor with parameters.
#endregion

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace AnyClone.Tests.TestObjects;

using System.Collections.ObjectModel;

[GenerateClone]
public class CustomCollectionObject<T> : Collection<T>
{
  public int CustomId { get; set; }
  public string CustomName { get; set; }
  public CustomCollectionObject(int customId, string customName)
  {
    CustomId = customId;
    CustomName = customName;
  }
}
