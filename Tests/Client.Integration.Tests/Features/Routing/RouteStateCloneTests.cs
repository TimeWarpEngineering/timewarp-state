namespace TestApp.Client.Integration.Tests.Features.Routing
{
  using AnyClone;
  using BlazorState;
  using BlazorState.Features.Routing;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using System;
  using TestApp.Client.Integration.Tests.Infrastructure;

  internal class RouteStateCloneTests
  {
    public RouteStateCloneTests(TestFixture aTestFixture)
    {
      IServiceProvider serviceProvider = aTestFixture.ServiceProvider;
      IStore store = serviceProvider.GetService<IStore>();
      RouteState = store.GetState<RouteState>();
    }

    private RouteState RouteState { get; set; }

    public void ShouldClone()
    {
      //Arrange
      RouteState.Initialize(aRoute:"SomeRoute");

      //Act
      RouteState clone = RouteState.Clone();

      //Assert
      RouteState.ShouldNotBeSameAs(clone);
      RouteState.Route.ShouldBe(clone.Route);
      RouteState.Guid.ShouldNotBe(clone.Guid);
    }

  }
}
