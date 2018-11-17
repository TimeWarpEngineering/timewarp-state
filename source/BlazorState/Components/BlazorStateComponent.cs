namespace BlazorState
{
  using MediatR;
  using Microsoft.AspNetCore.Blazor.Components;

  /// <summary>
  /// A simple non required Base Class that just injects Mediator and Store.
  /// And exposes StateHasChanged
  /// </summary>
  /// <remarks>Implements IBlazorStateComponent by Injecting</remarks>
  public class BlazorStateComponent : BlazorComponent,
     IBlazorStateComponent
  {
    [Inject] public IMediator Mediator { get; set; }
    [Inject] public IStore Store { get; set; }

    /// <summary>
    /// Exposes StateHasChanged
    /// </summary>
    public void ReRender() => StateHasChanged();

  }
}