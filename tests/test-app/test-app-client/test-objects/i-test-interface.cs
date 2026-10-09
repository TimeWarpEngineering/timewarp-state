#region Purpose
// Interface used by the clone tests to check members typed as an interface.
#endregion

#region Design
// Trivial test object.
#endregion

// ReSharper disable UnusedMemberInSuper.Global
namespace AnyClone.Tests.TestObjects;

public interface ITestInterface
{
  bool BoolValue { get; set; }
  int IntValue { get; set; }
  IDictionary<int, BasicObject> DictionaryValue { get; set; }
}
