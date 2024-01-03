namespace BlazorState.Features.Routing;
public partial class RouteState
{
  internal class InitializeRouteHandler : ActionHandler<InitializeRouteAction>
  {
    public InitializeRouteHandler
    (
      IStore aStore,
      NavigationManager aNavigationManager
    ) : base(aStore)
    {
      NavigationManager = aNavigationManager;
    }

    private RouteState RouteState => Store.GetStateAsync<RouteState>();

    private readonly NavigationManager NavigationManager;

    public override Task Handle(InitializeRouteAction aInitializeRouteRequest, CancellationToken aCancellationToken)
    {
      RouteState.Route = NavigationManager.Uri;
      return Task.CompletedTask;
    }
  }
}
