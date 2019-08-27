namespace TestApp.Client.Integration.Tests.Clone
{
  using Shouldly;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using AnyClone;
  using System.Linq;

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

  internal class CloneTests
  {
    public CloneTests(TestFixture aTestFixture)    {    }  

    public void ShouldCloneTestState()
    {

      // Arrange
      var testState = new TestState();

      // Act
      TestState clone = testState.Clone();

      // Assert
      clone.SortedFruits.Count().ShouldBe(7);
    }
  }
}
