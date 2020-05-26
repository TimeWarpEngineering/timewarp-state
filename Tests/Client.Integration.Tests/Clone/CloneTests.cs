namespace TestApp.Client.Integration.Tests.Clone_Tests
{
  using Shouldly;
  using AnyClone;
  using System.Linq;
  using TestApp.Client.Integration.Tests.Clone;

  internal class CloneTests
  {

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
