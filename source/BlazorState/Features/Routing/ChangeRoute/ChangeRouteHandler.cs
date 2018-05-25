namespace BlazorState.Features.Routing
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using Microsoft.AspNetCore.Blazor.Services;

  public class ChangeRouteHandler : RequestHandler<ChangeRouteRequest, RouteState>
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

    public override Task<RouteState> Handle(ChangeRouteRequest request, CancellationToken cancellationToken)
    {
      if (RouteState.Route != request.NewRoute)
      {
        RouteState.Route = request.NewRoute;
        UriHelper.NavigateTo(request.NewRoute);
      }
      return Task.FromResult(RouteState);
    }
  }
}