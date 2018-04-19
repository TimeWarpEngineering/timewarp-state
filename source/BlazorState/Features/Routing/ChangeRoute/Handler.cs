namespace BlazorState.Features.Routing.ChangeRoute
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
      if (RouteState.Route != request.NewRoute)
      {
        RouteState.Route = request.NewRoute;
        UriHelper.NavigateTo(request.NewRoute);
      }
      return Task.FromResult(RouteState);
    }
  }
}