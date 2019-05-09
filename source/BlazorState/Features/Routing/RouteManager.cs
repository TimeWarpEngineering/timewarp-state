namespace BlazorState.Features.Routing
{
  using MediatR;
  using Microsoft.AspNetCore.Components;

  /// <summary>
  /// When constructed will attach a OnLocationChanged Handler 
  /// to send ChangeRouteRequest
  /// </summary>
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
      Mediator.Send(new InitializeRouteAction());
    }

    private IMediator Mediator { get; }
    private RouteState RouteState => Store.GetState<RouteState>();
    private IStore Store { get; }
    private IUriHelper UriHelper { get; }

    private void OnLocationChanged(object aSender, string aNewLocation)
    {
      string absoluteUri = UriHelper.ToAbsoluteUri(aNewLocation).ToString();
      if (RouteState.Route != absoluteUri)
      {
        Mediator.Send(new ChangeRouteAction { NewRoute = absoluteUri });
      }
    }
  }
}