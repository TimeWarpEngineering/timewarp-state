namespace BlazorState.Features.Routing.InitializeRoute
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState.Handlers;
  using BlazorState.Store;
  using Microsoft.AspNetCore.Blazor.Services;

  public class Handler : RequestHandler<Request, RouteState>
  {
    public Handler(
      IStore aStore,
      IUriHelper aUriHelper
      ) : base(aStore)
    {
      UriHelper = aUriHelper;
    }

    private RouteState RouteState => Store.GetState<RouteState>();

    private IUriHelper UriHelper { get; }

    public override Task<RouteState> Handle(Request request, CancellationToken cancellationToken)
    {
      RouteState.Route = UriHelper.GetAbsoluteUri().ToString();
      return Task.FromResult(RouteState);
    }
  }
}