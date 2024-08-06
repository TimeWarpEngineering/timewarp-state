namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  private RenderReasonCategory RenderReason { get; set; } = RenderReasonCategory.None;
  private string? RenderReasonDetail { get; set; }

  public enum RenderReasonCategory
  {
    None,
    UntrackedParameter,
    ParameterChanged,
    Subscription,
    RenderTrigger
  }
}
