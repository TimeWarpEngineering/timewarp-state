namespace BlazorState.Features.Routing
{
  using MediatR;
  using Microsoft.AspNetCore.Blazor.Services;

  internal class RouteManager
  {
    public RouteManager(
      IUriHelper aUriHelper,
      IMediator aMediator,
      IStore aStore)
    {
      UriHelper = aUriHelper;
      Mediator = aMediator;
      Store = aStore;
      UriHelper.OnLocationChanged += OnLocationChanged;
      Mediator.Send(new InitializeRouteRequest());
    }

    private IMediator Mediator { get; }
    private RouteState RouteState => Store.GetState<RouteState>();
    private IStore Store { get; }
    private IUriHelper UriHelper { get; }

    private void OnLocationChanged(object sender, string e)
    {
      string absoluteUri = UriHelper.ToAbsoluteUri(e).ToString();
      if (RouteState.Route != absoluteUri)
        Mediator.Send(new ChangeRouteRequest { NewRoute = absoluteUri });
    }
  }
}