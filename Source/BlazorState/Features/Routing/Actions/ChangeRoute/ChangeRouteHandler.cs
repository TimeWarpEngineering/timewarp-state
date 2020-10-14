namespace BlazorState.Features.Routing
{
  using BlazorState;
  using MediatR;
  using Microsoft.AspNetCore.Components;
  using System.Threading;
  using System.Threading.Tasks;

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