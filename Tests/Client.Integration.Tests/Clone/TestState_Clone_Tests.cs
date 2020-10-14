namespace TestState
{
  using AnyClone;
  using Shouldly;
  using System.Linq;
  using TestApp.Client.Integration.Tests.Clone;

  public class Clone_Should
  {

    public void Clone()
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