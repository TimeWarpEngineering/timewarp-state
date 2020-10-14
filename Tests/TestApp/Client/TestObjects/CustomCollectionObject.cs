namespace AnyClone.Tests.TestObjects
{
  using System.Collections.ObjectModel;

  public class CustomCollectionObject<T> : Collection<T>
  {
    public int CustomId { get; set; }
    public string CustomName { get; set; }
    public CustomCollectionObject(int aCustomId, string aCustomName)
    {
      CustomId = aCustomId;
      CustomName = aCustomName;
    }
  }
}