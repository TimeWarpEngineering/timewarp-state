// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace AnyClone.Tests.TestObjects;

using System.Collections.ObjectModel;

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
