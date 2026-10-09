#region Purpose
// Checks that the deep cloner copes with a class exposing a LINQ IOrderedEnumerable property.
#endregion

#region Design
// Clones a plain TestState (not a store state) with the Clone() extension and checks the sorted view still yields all
// 7 fruits. Static method, so no host or store is involved.
#endregion

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
