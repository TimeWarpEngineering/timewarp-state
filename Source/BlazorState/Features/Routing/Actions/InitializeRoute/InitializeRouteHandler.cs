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
        IUriHelper aUriHelper
        ) : base(aStore)
      {
        UriHelper = aUriHelper;
      }

      private RouteState RouteState => Store.GetState<RouteState>();

      private IUriHelper UriHelper { get; }

      public override Task<RouteState> Handle(InitializeRouteAction aInitializeRouteRequest, CancellationToken aCancellationToken)
      {
        RouteState.Route = UriHelper.GetAbsoluteUri().ToString();
        return Task.FromResult(RouteState);
      }
    }
  }
}