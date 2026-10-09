#region Purpose
// Shared Class.Method formatting for the three render-caller diagnostic properties.
#endregion

#region Design
// Capture stays at each call site with GetFrame(1) so the frame is who invoked ShouldRender,
// SetParametersAsync, or StateHasChanged. CallerMemberName would name those methods themselves.
// TimeWarpStateOptions.CaptureRenderCaller (default false) skips StackTrace on the hot path;
// diagnostic pages that render *WasCalledBy must opt in.
#endregion

namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  [Inject] private TimeWarpStateOptions TimeWarpStateOptions { get; set; } = null!;

  private static string FormatRenderCaller(StackFrame? frame)
  {
    MethodBase? method = frame?.GetMethod();
    string className = method?.DeclaringType?.Name ?? "Unknown";
    string methodName = method?.Name ?? "Unknown";
    return $"{className}.{methodName}";
  }
}
