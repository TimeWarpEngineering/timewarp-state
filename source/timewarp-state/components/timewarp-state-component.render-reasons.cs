#region Purpose
// Exposes why TimeWarpStateComponent last rendered (RenderReason, RenderReasonDetail, ShouldRenderWasCalledBy).
#endregion

#region Design
// Diagnostic, publicly readable properties with private setters. RenderReasonCategory is a nested enum because it
// only describes this component's render cycle.
#endregion

namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  public RenderReasonCategory RenderReason { get; private set; } = RenderReasonCategory.None;
  public string? RenderReasonDetail { get; private set; }
  public string? ShouldRenderWasCalledBy  { get; private set; }

  public enum RenderReasonCategory
  {
    None,
    Event,
    ParameterChanged,
    UntrackedParameter,
    Subscription,
    RenderTrigger,
    StateHasChanged,
    Forced,
  }
}
