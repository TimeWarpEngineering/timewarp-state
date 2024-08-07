namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  public RenderReasonCategory RenderReason { get; private set; } = RenderReasonCategory.None;
  public string? RenderReasonDetail { get; private set; }

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
