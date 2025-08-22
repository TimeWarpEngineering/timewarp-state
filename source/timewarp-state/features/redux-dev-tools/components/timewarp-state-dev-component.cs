namespace TimeWarp.Features.ReduxDevTools;

/// <summary>
/// Adds a RenderModeDisplay RenderFragment to the TimeWarpStateComponent
/// </summary>
public class TimeWarpStateDevComponent : TimeWarpStateComponent
{
  protected readonly RenderFragment RenderModeDisplay;
  
  protected string RenderModeDisplayString => $"CurrentRenderMode: {CurrentRenderMode}\nConfiguredRenderMode: {ConfiguredRenderMode}\nRendererInfo.Name: {RendererInfo.Name}\nAssignedRenderMode: {AssignedRenderMode?.GetType().Name}\nRendererInfo.IsInteractive: {RendererInfo.IsInteractive}";
  protected TimeWarpStateDevComponent()
  {
    RenderModeDisplay = builder =>
    {
      builder.OpenComponent<RenderModeDisplay>(0);
      builder.AddComponentParameter(0, nameof(TimeWarp.Features.Developer.RenderModeDisplay.CurrentRenderMode), CurrentRenderMode);
      builder.AddComponentParameter(0, nameof(TimeWarp.Features.Developer.RenderModeDisplay.ConfiguredRenderMode), ConfiguredRenderMode);
      builder.AddComponentParameter(0, nameof(TimeWarp.Features.Developer.RenderModeDisplay.RendererInfoName), RendererInfo.Name);
      builder.AddComponentParameter(0, nameof(TimeWarp.Features.Developer.RenderModeDisplay.AssignedRenderMode), AssignedRenderMode?.GetType().Name);
      builder.AddComponentParameter(0, nameof(TimeWarp.Features.Developer.RenderModeDisplay.RendererInfoIsInteractive), RendererInfo.IsInteractive);
      builder.CloseComponent();
    };
  }
}
