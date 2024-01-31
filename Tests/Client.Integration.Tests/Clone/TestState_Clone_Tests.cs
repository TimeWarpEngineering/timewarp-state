namespace TestState_;

public class Clone_Should
{

  public void Clone()
  {

    // Arrange
    var testState = new TestState();

    // Act
    TestState clone = testState.Clone();

    // Assert
    clone.SortedFruits.Count().Should().Be(7);
  }
}
