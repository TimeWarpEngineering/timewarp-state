#region Purpose
// Checks that cloning ApplicationState copies Name and ExceptionMessage and gives the clone a new Guid.
#endregion

#region Design
// Seeds the state through its test-only Initialize(name, exceptionMessage), which ThrowIfNotTestAssembly allows from
// this assembly, then calls the Clone() extension directly rather than going through an action.
#endregion

// ReSharper disable UnusedType.Global
namespace ApplicationState_;

public class Clone_Should : BaseTest
{
  public Clone_Should(ClientHost clientHost) : base(clientHost)
  {
    ApplicationState = Store.GetState<ApplicationState>();
  }

  private ApplicationState ApplicationState { get; }
  
  public void Clone()
  {
    //Arrange
    ApplicationState.Initialize(name: "TestName", exceptionMessage: "Some ExceptionMessage");

    //Act
    ApplicationState clone = ApplicationState.Clone();

    //Assert
    ApplicationState.ShouldNotBeSameAs(clone);
    ApplicationState.Name.ShouldBe(clone.Name);
    ApplicationState.ExceptionMessage.ShouldBe(clone.ExceptionMessage);
    ApplicationState.Guid.ShouldNotBe(clone.Guid);
  }
}
