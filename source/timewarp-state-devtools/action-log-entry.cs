#region Purpose
// One row in the DevTools action log.
#endregion

#region Design
// Immutable value copied from a span ingest item. It is not an IState. The list lives on
// InspectorLog, not on InspectorState, because the HTTP ingest scope and the Blazor circuit
// do not share a scoped store.
#endregion

namespace TimeWarp.State.DevTools;

/// <summary>
/// A captured TimeWarp.State span.
/// </summary>
public sealed class ActionLogEntry
{
  /// <summary>Monotonic id assigned by <see cref="InspectorLog"/>.</summary>
  public required long Sequence { get; init; }

  /// <summary>Activity name.</summary>
  public required string Name { get; init; }

  /// <summary>Value of <c>timewarp.state.action</c>, or the activity name.</summary>
  public required string ActionName { get; init; }

  /// <summary>Value of <c>timewarp.state.state_type</c>, or empty.</summary>
  public required string StateTypeName { get; init; }

  /// <summary>Activity status name.</summary>
  public required string Status { get; init; }

  /// <summary>Duration in milliseconds.</summary>
  public required double DurationMilliseconds { get; init; }

  /// <summary>UTC start.</summary>
  public required DateTime StartTimeUtc { get; init; }

  /// <summary>W3C span id.</summary>
  public string SpanId { get; init; } = "";

  /// <summary>Latest snapshot or diff JSON, when the span carried one.</summary>
  public string? SnapshotJson { get; init; }

  /// <summary><c>state.snapshot</c> or <c>state.diff</c>, when JSON is present.</summary>
  public string? SnapshotKind { get; init; }

  /// <summary>True when the snapshot event set <c>snapshot.truncated</c>.</summary>
  public bool SnapshotTruncated { get; init; }
}
