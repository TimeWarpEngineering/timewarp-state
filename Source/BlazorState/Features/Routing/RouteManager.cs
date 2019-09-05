namespace BlazorState.Features.Routing
{
  using System;
  using MediatR;
  using Microsoft.AspNetCore.Components;
  using Microsoft.AspNetCore.Components.Routing;

  /// <summary>
  /// When constructed will attach a OnLocationChanged Handler 
  /// to send ChangeRouteRequest
  /// </summary>
  public class RouteManager
  {
    public RouteManager(
      NavigationManager aNavigationManager,
      IMediator aMediator,
      IStore aStore)
    {
      NavigationManager = aNavigationManager;
      Mediator = aMediator;
      Store = aStore;
      NavigationManager.LocationChanged += LocationChanged;
      Mediator.Send(new InitializeRouteAction());
    }

    private IMediator Mediator { get; }
    private RouteState RouteState => Store.GetState<RouteState>();
    private IStore Store { get; }
    private NavigationManager NavigationManager { get; }

    private void LocationChanged(object aSender, LocationChangedEventArgs aLocationChangedEventArgs)
    {
      string absoluteUri = NavigationManager.ToAbsoluteUri(aLocationChangedEventArgs.Location).ToString();
      if (RouteState.Route != absoluteUri)
      {
        Mediator.Send(new ChangeRouteAction { NewRoute = absoluteUri });
      }
    }
  }
}