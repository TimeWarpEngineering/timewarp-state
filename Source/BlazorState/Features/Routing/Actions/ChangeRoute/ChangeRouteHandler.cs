namespace BlazorState.Features.Routing
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using Microsoft.AspNetCore.Components;

  public partial class RouteState
  {
    internal class ChangeRouteHandler : RequestHandler<ChangeRouteAction, RouteState>
    {
      public ChangeRouteHandler(
        IStore aStore,
        NavigationManager aNavigationManager
        ) : base(aStore)
      {
        NavigationManager = aNavigationManager;
      }

      private RouteState RouteState => Store.GetState<RouteState>();

      private NavigationManager NavigationManager { get; }

      public override Task<RouteState> Handle(ChangeRouteAction aChangeRouteRequest, CancellationToken aCancellationToken)
      {
        string newAbsoluteUri = NavigationManager.ToAbsoluteUri(aChangeRouteRequest.NewRoute).ToString();
        string absoluteUri = NavigationManager.Uri;

        if (absoluteUri != newAbsoluteUri)
        {
          // RouteManager OnLocationChanged will fire this ChangeRouteRequest again 
          // and the second time we will hit the else clause.
          NavigationManager.NavigateTo(newAbsoluteUri);
        }
        else if (RouteState.Route != newAbsoluteUri)
        {
          RouteState.Route = newAbsoluteUri;
        }

        return Task.FromResult(RouteState);
      }
    }
  }
}