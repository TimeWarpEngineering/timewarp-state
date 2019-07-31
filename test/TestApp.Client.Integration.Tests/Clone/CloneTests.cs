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
      string[] fruits = { "apricot", "orange", "banana", "mango", "apple", "grape", "strawberry" };

      // Sort the strings first by their length and then alphabetically
      // by passing the identity selector function.
      SortedFruits = fruits.OrderBy(aFruit => aFruit.Length).ThenBy(aFruit => aFruit);
    }
    
    public IOrderedEnumerable<string> SortedFruits { get; set; }
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
