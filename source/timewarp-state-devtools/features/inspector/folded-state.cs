#region Purpose
// One state's latest snapshot inside a fold of the action log.
#endregion

#region Design
// Immutable value so InspectorLog can hand it out under its lock without the caller seeing later writes.
// Json is the full stored snapshot, including a truncated payload the tree refuses to parse.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

/// <summary>
/// Latest snapshot for one state type at a point in the log.
/// </summary>
public sealed class FoldedState
{
  /// <summary>Simple state type name (<c>timewarp.state.state_type</c>).</summary>
  public required string StateTypeName { get; init; }

  /// <summary>Sequence of the entry this snapshot came from.</summary>
  public required long Sequence { get; init; }

  /// <summary>Snapshot JSON. Empty when the entry carried none.</summary>
  public required string Json { get; init; }

  /// <summary>True when the payload was cut at <c>MaxSnapshotChars</c> or at ingest.</summary>
  public required bool Truncated { get; init; }
}
