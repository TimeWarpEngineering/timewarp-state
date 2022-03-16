namespace ApplicationState;

using AnyClone;
using FluentAssertions;
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
    ApplicationState.Should().NotBeSameAs(clone);
    ApplicationState.Name.Should().Be(clone.Name);
    ApplicationState.ExceptionMessage.Should().Be(clone.ExceptionMessage);
    ApplicationState.Guid.Should().NotBe(clone.Guid);
  }

}
