using System.Collections.ObjectModel;

namespace AnyClone.Tests.TestObjects
{
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
}
