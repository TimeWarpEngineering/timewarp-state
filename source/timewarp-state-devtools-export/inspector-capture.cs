#region Purpose
// Decides which TimeWarp.State spans the DevTools channel keeps.
#endregion

#region Design
// Tag names match TimeWarp.State.Telemetry's span tags. InspectorState is the DevTools UI state
// simple name (DeclaringType.Name). Dropping that name, and timewarp.state.inspector=true, stops a
// miswired exporter from feeding the inspector's own actions back into the log it displays.
#endregion

namespace TimeWarp.State.DevTools.Export;

/// <summary>
/// Capture rules for the DevTools observation channel.
/// </summary>
public static class InspectorCapture
{
  /// <summary>ActivitySource name. Matches <c>TimeWarpStateTelemetry.ActivitySourceName</c>.</summary>
  public const string ActivitySourceName = "TimeWarp.State";

  /// <summary>Span tag: action name relative to the enclosing state.</summary>
  public const string ActionTagName = "timewarp.state.action";

  /// <summary>Span tag: enclosing state type simple name.</summary>
  public const string StateTypeTagName = "timewarp.state.state_type";

  /// <summary>Span tag: when <c>true</c>, the span is the inspector's own action and is dropped.</summary>
  public const string InspectorTagName = "timewarp.state.inspector";

  /// <summary>Snapshot and diff event payload tag.</summary>
  public const string SnapshotJsonTagName = "snapshot.json";

  /// <summary>Snapshot and diff event truncation tag.</summary>
  public const string SnapshotTruncatedTagName = "snapshot.truncated";

  /// <summary>First JSON event name emitted by TimeWarp.State.Telemetry.</summary>
  public const string SnapshotEventName = "state.snapshot";

  /// <summary>Later unequal JSON event name emitted by TimeWarp.State.Telemetry.</summary>
  public const string DiffEventName = "state.diff";

  /// <summary>Span tag: <see cref="InitKind"/> on a state-initialization activity.</summary>
  public const string KindTagName = "timewarp.state.kind";

  /// <summary>Value of <see cref="KindTagName"/> for a snapshot emitted when a state is initialized.</summary>
  public const string InitKind = "init";

  /// <summary>Simple name of the DevTools UI state. Spans for this state are not captured.</summary>
  public const string InspectorStateTypeName = "InspectorState";

  /// <summary>
  /// Returns false for inspector spans. Other spans are captured, including actions with no enclosing state.
  /// </summary>
  /// <param name="stateTypeName">Value of <see cref="StateTypeTagName"/>, or null.</param>
  /// <param name="inspectorTag">Value of <see cref="InspectorTagName"/>, or null.</param>
  public static bool ShouldCapture(string? stateTypeName, string? inspectorTag)
  {
    if (string.Equals(inspectorTag, "true", StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    if (string.Equals(stateTypeName, InspectorStateTypeName, StringComparison.Ordinal))
    {
      return false;
    }

    return true;
  }
}
