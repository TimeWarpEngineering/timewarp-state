#region Purpose
// Plain object for the clone test: a fruit array plus a computed, sorted IOrderedEnumerable view.
#endregion

#region Design
// [NotTest] keeps Fixie from treating it as a test class; it is not a TimeWarp.State state.
#endregion

namespace TestApp.Client.Integration.Tests.Clone;


[GenerateClone]
[NotTest]
public class TestState
{
  // Create an array of strings to sort.
  public string[] Fruits { get; set; } = ["apricot", "orange", "banana", "mango", "apple", "grape", "strawberry"];
  public IOrderedEnumerable<string> SortedFruits => Fruits.OrderBy(fruit => fruit.Length).ThenBy(fruit => fruit);
}
