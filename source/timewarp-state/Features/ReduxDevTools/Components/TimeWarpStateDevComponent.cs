namespace TimeWarp.Features.ReduxDevTools;

/// <summary>
/// Adds a RenderModeDisplay RenderFragment to the TimeWarpStateComponent
/// </summary>
public class TimeWarpStateDevComponent : TimeWarpStateComponent
{
  protected readonly RenderFragment RenderModeDisplay;
  
  protected string RenderModeDisplayString => $"CurrentRenderMode: {CurrentRenderMode}\nConfiguredRenderMode: {ConfiguredRenderMode}";
  protected TimeWarpStateDevComponent()
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
