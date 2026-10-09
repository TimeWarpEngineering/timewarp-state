#region Purpose
// Snapshot of a stopped Activity for assertions.
#endregion

#region Design
// Copies the fields tests check (name, status, tags, events, exception flag) when the activity stops, so assertions
// read plain values.
#endregion

namespace TimeWarp.State.Telemetry.Tests;

internal sealed class RecordedActivity
{
  public required string DisplayName { get; init; }
  public required ActivityStatusCode Status { get; init; }
  public string? StatusDescription { get; init; }
  public required List<KeyValuePair<string, object?>> Tags { get; init; }
  public required List<ActivityEvent> Events { get; init; }
  public required bool HasExceptionEvent { get; init; }
}
