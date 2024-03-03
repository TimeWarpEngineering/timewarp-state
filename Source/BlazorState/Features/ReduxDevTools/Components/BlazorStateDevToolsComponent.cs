namespace TimeWarp.Features.ReduxDevTools;

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
  protected readonly RenderFragment RenderModeDisplay;
  protected override void OnInitialized()
  {
    base.OnInitialized();
    Subscriptions.Add<IDevToolsComponent>(this);
  }
  
  protected string RenderModeDisplayString => $"CurrentRenderMode: {CurrentRenderMode}\nConfiguredRenderMode: {ConfiguredRenderMode}";
  protected BlazorStateDevToolsComponent()
  {
    RenderModeDisplay = builder =>
    {
      builder.OpenComponent<RenderModeDisplay>(0);
      builder.AddComponentParameter(0, nameof(TimeWarp.Features.Developer.RenderModeDisplay.CurrentRenderMode), CurrentRenderMode);
      builder.AddComponentParameter(0, nameof(TimeWarp.Features.Developer.RenderModeDisplay.ConfiguredRenderMode), ConfiguredRenderMode);
      builder.CloseComponent();
    };
  }
}
