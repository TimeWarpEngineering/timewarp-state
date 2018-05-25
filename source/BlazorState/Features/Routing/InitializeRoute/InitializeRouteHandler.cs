namespace BlazorState.Features.Routing
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using Microsoft.AspNetCore.Blazor.Services;

  public class InitializeRouteHandler : RequestHandler<InitializeRouteRequest, RouteState>
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

    public override Task<RouteState> Handle(InitializeRouteRequest request, CancellationToken cancellationToken)
    {
      RouteState.Route = UriHelper.GetAbsoluteUri().ToString();
      return Task.FromResult(RouteState);
    }
  }
}