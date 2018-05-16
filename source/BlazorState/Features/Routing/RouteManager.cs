namespace BlazorState.Features.Routing
{
  using BlazorState.Store;
  using MediatR;
  using Microsoft.AspNetCore.Blazor.Services;

  public class RouteManager
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
      Mediator.Send(new Routing.InitializeRoute.Request());
    }

    private IMediator Mediator { get; }
    private IStore Store { get; }
    private IUriHelper UriHelper { get; }
    private RouteState RouteState => Store.GetState<RouteState>();

    private void OnLocationChanged(object sender, string e)
    {
      string absoluteUri = UriHelper.ToAbsoluteUri(e).ToString();
      if(RouteState.Route != absoluteUri)
        Mediator.Send(new Routing.ChangeRoute.Request { NewRoute = absoluteUri });
    }
  }
}