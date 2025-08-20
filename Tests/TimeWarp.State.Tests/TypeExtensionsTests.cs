namespace GetEnclosingStateType_;

using TimeWarp.Features.RenderSubscriptions;
using TimeWarp.State.Extensions;

public class TypeExtensionsTests
{
  private class TestState : IState
  {
    public class NestedClass;

    public class AnotherNestedClass : NestedClass;

    public ISender Sender { get; set; } = null!;
    public Guid Guid { get; } = Guid.Empty;
    public void Initialize() => throw new NotImplementedException();
    public void CancelOperations() => throw new NotImplementedException();
  }
    
  public void Should_Get_Enclosing_State_Type_For_Nested_Class()
  {
    // Arrange
    Type nestedClassType = typeof(TestState.NestedClass);

    // Act
    Type enclosingStateType = nestedClassType.GetEnclosingStateType();

    // Assert
    enclosingStateType.ShouldBe(typeof(TestState));
  }
    
  public void Should_Get_Enclosing_State_Type_For_Deeply_Nested_Class()
  {
    // Arrange
    Type deeplyNestedClassType = typeof(TestState.AnotherNestedClass);

    // Act
    Type enclosingStateType = deeplyNestedClassType.GetEnclosingStateType();

    // Assert
    enclosingStateType.ShouldBe(typeof(TestState));
  }
    
  public void Should_Throw_Exception_For_Non_Nested_Class()
  {
    // Arrange
    Type nonNestedClassType = typeof(string); // Example of a non-nested class

    // Act & Assert
    Should.Throw<NonNestedClassException>(() => nonNestedClassType.GetEnclosingStateType())
      .Message.ShouldBe("String must be nested in a class that implements IState");
  }
}
