namespace TestApp.Client.Integration.Tests.Clone
{
  using System.Linq;
  using TestApp.Client.Integration.Tests.Infrastructure;

  [NotTest]
  public class TestState
  {
    public TestState()
    {
      // Create an array of strings to sort.
      Fruits = new string[] { "apricot", "orange", "banana", "mango", "apple", "grape", "strawberry" };
    }

    public string[] Fruits { get; set; }
    public IOrderedEnumerable<string> SortedFruits => Fruits.OrderBy(aFruit => aFruit.Length).ThenBy(aFruit => aFruit);
  }
}