namespace AnyClone.Tests.TestObjects;

public interface ITestInterface
{
  bool BoolValue { get; set; }
  int IntValue { get; set; }
  IDictionary<int, BasicObject> DictionaryValue { get; set; }
}
