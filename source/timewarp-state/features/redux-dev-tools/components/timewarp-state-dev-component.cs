namespace TimeWarp.Features.ReduxDevTools;

/// <summary>
/// Adds a RenderModeDisplay RenderFragment to the TimeWarpStateComponent
/// </summary>
public class TimeWarpStateDevComponent : TimeWarpStateComponent
{
  protected readonly RenderFragment RenderModeDisplay;
  
  protected string RenderModeDisplayString => $"RendererInfo.Name: {RendererInfo.Name}\nAssignedRenderMode: {AssignedRenderMode?.GetType().Name}\nRendererInfo.IsInteractive: {RendererInfo.IsInteractive}";
  protected TimeWarpStateDevComponent()
  {
    RenderModeDisplay = builder =>
    {
      builder.OpenComponent<Features.Developer.RenderModeDisplay>(0);
      builder.AddComponentParameter(0, nameof(Features.Developer.RenderModeDisplay.RendererInfoName), RendererInfo.Name);
      builder.AddComponentParameter(0, nameof(Features.Developer.RenderModeDisplay.AssignedRenderModeName), AssignedRenderMode?.GetType().Name);
      builder.AddComponentParameter(0, nameof(Features.Developer.RenderModeDisplay.RendererInfoIsInteractive), RendererInfo.IsInteractive);
      builder.CloseComponent();
    };
  }
}
