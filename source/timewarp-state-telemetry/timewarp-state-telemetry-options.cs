#region Purpose
// Host-facing options for opt-in snapshots; default spans need no configuration.
#endregion

#region Design
// IncludeSnapshots defaults false so the hot path never serializes. JsonSerializerOptions is
// caller-supplied (or TimeWarpStateOptions after 065); this type never constructs options.
// MaxSnapshotChars caps span-event payload size when snapshots are enabled.
#endregion

namespace TimeWarp.State.Telemetry;

/// <summary>
/// Options for <see cref="TelemetryBehavior{TRequest,TResponse}"/>.
/// </summary>
public sealed class TimeWarpStateTelemetryOptions
{
  /// <summary>
  /// When true, attach state snapshots and later diffs as span events. Requires a listener,
  /// a sampled span (<c>IsAllDataRequested</c>), and <see cref="JsonTypeInfo"/> for the state
  /// type from <see cref="JsonSerializerOptions"/> (or <see cref="TimeWarpStateOptions.JsonSerializerOptions"/>).
  /// Default is false: metadata-only spans with no payload.
  /// </summary>
  public bool IncludeSnapshots { get; set; }

  /// <summary>
  /// Serializer used for opt-in snapshots. When null, <see cref="TimeWarpStateOptions.JsonSerializerOptions"/>
  /// is used. The options must expose a <see cref="JsonSerializerOptions.TypeInfoResolver"/> that can
  /// produce <see cref="JsonTypeInfo"/> for each state type. Never constructed by this package.
  /// </summary>
  public JsonSerializerOptions? JsonSerializerOptions { get; set; }

  /// <summary>
  /// Maximum characters written into a snapshot or diff span event. Longer JSON is truncated.
  /// </summary>
  public int MaxSnapshotChars { get; set; } = 16_384;
}
