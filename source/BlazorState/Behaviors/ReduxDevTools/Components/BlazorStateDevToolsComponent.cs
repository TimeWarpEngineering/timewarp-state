namespace BlazorState.Behaviors.ReduxDevTools
{
  using BlazorState;
  using Microsoft.AspNetCore.Components;

  /// <summary>
  /// Base implementation of IDevToolsComponent. Required for TimeTravel in ReduxDevTools
  /// </summary>
  /// <remarks>See Peter Morris Issue on Blazor
  /// https://github.com/aspnet/Blazor/issues/704
  /// If one implements their own base class with these interfaces
  /// They won't be forced to use this one.
  /// C# 8 with default implementations of interfaces will be quite tempting to solve this.
  /// </remarks>
  public class BlazorStateDevToolsComponent : BlazorStateComponent,
    IDevToolsComponent
  {
    protected override void OnInit() => Subscriptions.Add<IDevToolsComponent>(this);
  }
}