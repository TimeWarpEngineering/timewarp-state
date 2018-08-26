namespace BlazorState.Features.Routing
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using Microsoft.AspNetCore.Blazor.Services;

  internal class ChangeRouteHandler : RequestHandler<ChangeRouteRequest, RouteState>
  {
    public ChangeRouteHandler(
      IStore aStore,
      IUriHelper aUriHelper
      ) : base(aStore)
    {
      UriHelper = aUriHelper;
    }

    private RouteState RouteState => Store.GetState<RouteState>();

    private IUriHelper UriHelper { get; }

    public override Task<RouteState> Handle(ChangeRouteRequest aChangeRouteRequest, CancellationToken aCancellationToken)
    {
      if (RouteState.Route != aChangeRouteRequest.NewRoute)
      {
        RouteState.Route = aChangeRouteRequest.NewRoute;
        UriHelper.NavigateTo(aChangeRouteRequest.NewRoute);
      }
      return Task.FromResult(RouteState);
    }
  }
}