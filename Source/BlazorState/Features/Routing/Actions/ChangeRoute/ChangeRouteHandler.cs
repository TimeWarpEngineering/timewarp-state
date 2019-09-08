namespace BlazorState.Features.Routing
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using MediatR;
  using Microsoft.AspNetCore.Components;

  public partial class RouteState
  {
    internal class ChangeRouteHandler : ActionHandler<ChangeRouteAction>
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

      public override Task<Unit> Handle(ChangeRouteAction aChangeRouteRequest, CancellationToken aCancellationToken)
      {
        string newAbsoluteUri = NavigationManager.ToAbsoluteUri(aChangeRouteRequest.NewRoute).ToString();
        string absoluteUri = NavigationManager.Uri;

        if (absoluteUri != newAbsoluteUri)
        {
          // RouteManager OnLocationChanged will fire this ChangeRouteRequest again 
          // and the second time we will hit the `else` clause.
          NavigationManager.NavigateTo(newAbsoluteUri);
        }
        else if (RouteState.Route != newAbsoluteUri)
        {
          RouteState.Route = newAbsoluteUri;
        }
        return Unit.Task;
      }
    }
  }
}