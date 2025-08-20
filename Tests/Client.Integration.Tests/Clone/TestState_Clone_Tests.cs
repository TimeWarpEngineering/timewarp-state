namespace TestState_;

using TestApp.Client.Integration.Tests.Clone;

public class Clone_Should
{

  public static void Clone()
  {

    // Arrange
    var testState = new TestState();

    // Act
    TestState clone = testState.Clone();

    // Assert
    clone.SortedFruits.Count().ShouldBe(7);
  }
}
