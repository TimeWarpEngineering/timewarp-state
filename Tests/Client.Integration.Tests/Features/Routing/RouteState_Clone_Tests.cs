namespace RouteState;

using AnyClone;
using BlazorState.Features.Routing;
  using FluentAssertions;
using TestApp.Client.Integration.Tests.Infrastructure;

public class Clone_Should : BaseTest
{
  public Clone_Should(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost)
  {
    RouteState = Store.GetState<RouteState>();
  }

  private RouteState RouteState { get; set; }

  public void ShouldClone()
  {
    //Arrange
    RouteState.Initialize(aRoute: "SomeRoute");

    //Act
    RouteState clone = RouteState.Clone();

    //Assert
    RouteState.Should().NotBeSameAs(clone);
    RouteState.Route.Should().Be(clone.Route);
    RouteState.Guid.Should().NotBe(clone.Guid);
  }

}
