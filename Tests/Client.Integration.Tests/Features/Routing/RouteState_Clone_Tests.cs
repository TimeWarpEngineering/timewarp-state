namespace RouteState
{
  using AnyClone;
  using BlazorState.Features.Routing;
  using Shouldly;
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
      RouteState.ShouldNotBeSameAs(clone);
      RouteState.Route.ShouldBe(clone.Route);
      RouteState.Guid.ShouldNotBe(clone.Guid);
    }

  }
}