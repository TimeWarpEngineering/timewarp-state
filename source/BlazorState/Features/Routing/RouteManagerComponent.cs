namespace BlazorState.Features.Routing
{
  using Microsoft.AspNetCore.Blazor.Components;

  /// <summary>
  /// Adds Routing into Blazor-State
  /// </summary>
  /// <remarks>Implements IBlazorStateComponent by Injecting</remarks>
  public class RouteManagerComponent : BlazorComponent
  {
    [Inject] private RouteManager RouteManager { get; set; }
  }
}