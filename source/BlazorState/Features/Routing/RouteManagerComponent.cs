namespace BlazorState.Features.Routing
{
  using BlazorState.Features.Routing;
  using Microsoft.AspNetCore.Blazor.Components;
  using Microsoft.AspNetCore.Blazor.RenderTree;

  /// <summary>
  /// A simple non required Base Class that just injects Mediator and Store
  /// </summary>
  /// <remarks>Implements IBlazorStateComponent by Injecting</remarks>
  public class RouteManagerComponent : BlazorComponent
  {
    [Inject] RouteManager RouteManager { get; set; }

  }
}