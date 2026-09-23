#region Purpose
// Records each successful ShouldRender so a later pass cannot hide the category.
#endregion

#region Design
// The line is appended inside ShouldRender, before the render reads the list. OnAfterRender is too late:
// the markup for this pass is already built, and the base method clears RenderReason there.
// CaptureRenderCaller records GetFrame(1). An override is that frame, so this type takes its own frame
// first and only when the flag is on. The column shows that frame, not ShouldRenderWasCalledBy.
// A skipped pass does not append, and the markup does not update. That silence is the skip.
// The cap keeps a long burst from growing the list without bound. The render count is the full total.
#endregion

namespace Sample06Wasm.Components;

/// <summary>
/// Keeps the last successful <c>ShouldRender</c> categories for the sample page.
/// </summary>
public class TracingComponent : TimeWarpStateComponent
{
  private readonly List<string> TraceList = [];

  [Inject] private TimeWarpStateOptions Options { get; set; } = null!;

  protected IReadOnlyList<string> Trace => TraceList;

  protected string? RecordedCaller { get; private set; }

  protected override bool ShouldRender()
  {
    if (Options.CaptureRenderCaller)
    {
      RecordedCaller = DescribeCaller(new StackTrace().GetFrame(1));
    }

    bool shouldRender = base.ShouldRender();
    if (!shouldRender)
    {
      return false;
    }

    string detail = string.IsNullOrEmpty(RenderReasonDetail) ? "" : $" ({RenderReasonDetail})";
    string caller = RecordedCaller is null ? "" : $" via {RecordedCaller}";
    TraceList.Add($"{RenderReason}{detail}{caller}");
    while (TraceList.Count > 40)
    {
      TraceList.RemoveAt(0);
    }

    return true;
  }

  private static string DescribeCaller(StackFrame? frame)
  {
    MethodBase? method = frame?.GetMethod();
    string className = method?.DeclaringType?.Name ?? "Unknown";
    string methodName = method?.Name ?? "Unknown";
    return $"{className}.{methodName}";
  }
}
