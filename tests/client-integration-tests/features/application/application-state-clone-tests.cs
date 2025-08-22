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
