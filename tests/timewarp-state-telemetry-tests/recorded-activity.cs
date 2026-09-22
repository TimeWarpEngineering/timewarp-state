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
