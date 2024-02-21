namespace BlazorState.Features.Routing;
public partial class RouteState
{
  internal class InitializeRouteHandler
  (
    IStore store,
    NavigationManager NavigationManager
  ) : ActionHandler<InitializeRoute.Action>(store)
  {
    private RouteState RouteState => Store.GetState<RouteState>();

    public override Task Handle(InitializeRoute.Action aInitializeRouteRequest, CancellationToken aCancellationToken)
    {
      RouteState.Route = NavigationManager.Uri;
      return Task.CompletedTask;
    }
  }
}
