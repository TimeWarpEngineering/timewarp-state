namespace BlazorState.Features.Routing
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using Microsoft.AspNetCore.Components;

  public partial class RouteState
  {
    internal class InitializeRouteHandler : RequestHandler<InitializeRouteAction, RouteState>
    {
      public InitializeRouteHandler(
        IStore aStore,
        NavigationManager aNavigationManager
        ) : base(aStore)
      {
        NavigationManager = aNavigationManager;
      }

      private RouteState RouteState => Store.GetState<RouteState>();

      private NavigationManager NavigationManager { get; }

      public override Task<RouteState> Handle(InitializeRouteAction aInitializeRouteRequest, CancellationToken aCancellationToken)
      {
        RouteState.Route = NavigationManager.Uri;
        return Task.FromResult(RouteState);
      }
    }
  }
}