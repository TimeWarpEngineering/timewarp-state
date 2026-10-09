#region Purpose
// Logging EventIds for TimeWarp.State.Telemetry.
#endregion

#region Design
// Internal static class using the 2000+ range so ids do not collide with core (100-801) or Plus (1000+).
#endregion

namespace TimeWarp.State.Telemetry;

internal static class EventIds
{
  public static readonly EventId TelemetryBehavior_Constructing = new(2000, nameof(TelemetryBehavior_Constructing));
  public static readonly EventId TelemetryBehavior_SnapshotSkipped = new(2001, nameof(TelemetryBehavior_SnapshotSkipped));
  public static readonly EventId TelemetryBehavior_SnapshotFailed = new(2002, nameof(TelemetryBehavior_SnapshotFailed));
}
