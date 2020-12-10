namespace ApplicationState
{
  using AnyClone;
  using Shouldly;
  using TestApp.Client.Features.Application;
  using TestApp.Client.Integration.Tests.Infrastructure;

  public class Clone_Should : BaseTest
  {
    public Clone_Should(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost)
    {
      ApplicationState = Store.GetState<ApplicationState>();
    }

    private ApplicationState ApplicationState { get; set; }

    public void Clone()
    {
      //Arrange
      ApplicationState.Initialize(aName: "TestName", aExceptionMessage: "Some ExceptionMessage");

      //Act
      ApplicationState clone = ApplicationState.Clone();

      //Assert
      ApplicationState.ShouldNotBeSameAs(clone);
      ApplicationState.Name.ShouldBe(clone.Name);
      ApplicationState.ExceptionMessage.ShouldBe(clone.ExceptionMessage);
      ApplicationState.Guid.ShouldNotBe(clone.Guid);
    }

  }
}