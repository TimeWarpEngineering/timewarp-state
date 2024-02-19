namespace BlazorState.Features.Routing;

public partial class RouteState
{
  internal class ChangeRouteHandler : ActionHandler<ChangeRouteAction>
  {
    private readonly NavigationManager NavigationManager;

    private RouteState RouteState => Store.GetState<RouteState>();

    public ChangeRouteHandler
    (
      IStore aStore,
      NavigationManager aNavigationManager
    ) : base(aStore)
    {
      NavigationManager = aNavigationManager;
    }

    public override Task Handle(ChangeRouteAction aChangeRouteRequest, CancellationToken aCancellationToken)
    {
      Console.WriteLine("ChangeRouteAction.Handle-NewRoute:" + aChangeRouteRequest.NewRoute);
      string newAbsoluteUri = NavigationManager.ToAbsoluteUri(aChangeRouteRequest.NewRoute).ToString();
      string absoluteUri = NavigationManager.Uri;

      if (absoluteUri != newAbsoluteUri)
      {
        // TimeWarpNavigationManager OnLocationChanged will fire this ChangeRouteRequest again
        // and the second time we will hit the `else` clause.
        NavigationManager.NavigateTo(newAbsoluteUri);
      }
      else if (RouteState.Route != newAbsoluteUri)
      {
        if(!RouteState.GoingBack)
          RouteState.HistoryStack.Push(RouteState.Route);
        else
        {
          RouteState.GoingBack = false;
        }
        RouteState.Route = newAbsoluteUri;
      }
      return Task.CompletedTask;
    }
  }
}
