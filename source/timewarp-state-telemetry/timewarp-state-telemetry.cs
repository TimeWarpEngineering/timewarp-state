#region Purpose
// Public ActivitySource consumers register with OpenTelemetry AddSource.
#endregion

namespace TimeWarp.State.Telemetry;

/// <summary>
/// OpenTelemetry source for TimeWarp.State action spans.
/// </summary>
/// <remarks>
/// Register the source in the host:
/// <c>tracing.AddSource(TimeWarpStateTelemetry.ActivitySourceName)</c>.
/// </remarks>
public static class TimeWarpStateTelemetry
{
  /// <summary>
  /// Activity source name. Pass this to <c>AddSource</c>.
  /// </summary>
  public const string ActivitySourceName = "TimeWarp.State";

  /// <summary>
  /// Activity source used by <see cref="TelemetryBehavior{TRequest,TResponse}"/>.
  /// </summary>
  public static ActivitySource ActivitySource { get; } = new(ActivitySourceName);
}
