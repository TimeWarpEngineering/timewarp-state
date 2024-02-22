namespace TestApp.Client.Integration.Tests.Clone;


[NotTest]
public class TestState
{
  // Create an array of strings to sort.
  public string[] Fruits { get; set; } = ["apricot", "orange", "banana", "mango", "apple", "grape", "strawberry"];
  public IOrderedEnumerable<string> SortedFruits => Fruits.OrderBy(aFruit => aFruit.Length).ThenBy(aFruit => aFruit);
}
