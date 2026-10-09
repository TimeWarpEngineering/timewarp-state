#region Purpose
// Test ActivityListener that records every stopped TimeWarp.State activity.
#endregion

#region Design
// Listens only to TimeWarpStateTelemetry.ActivitySourceName with a configurable sampling result, so tests can also
// check the not-sampled path. Disposing it detaches the listener.
#endregion

namespace TimeWarp.State.Telemetry.Tests;

internal sealed class ActivityListenerHarness : IDisposable
{
  private readonly ActivityListener ActivityListener;
  public List<RecordedActivity> Activities { get; } = [];

  public ActivityListenerHarness(ActivitySamplingResult activitySamplingResult = ActivitySamplingResult.AllDataAndRecorded)
  {
    ActivityListener = new ActivityListener
    {
      ShouldListenTo = activitySource => activitySource.Name == TimeWarpStateTelemetry.ActivitySourceName,
      Sample = (ref ActivityCreationOptions<ActivityContext> _) => activitySamplingResult,
      ActivityStopped = activity =>
      {
        Activities.Add
        (
          new RecordedActivity
          {
            DisplayName = activity.DisplayName,
            Status = activity.Status,
            StatusDescription = activity.StatusDescription,
            Tags = [.. activity.TagObjects],
            Events = [.. activity.Events],
            HasExceptionEvent = activity.Events.Any(activityEvent =>
              activityEvent.Name is "exception" ||
              activityEvent.Tags.Any(tag => tag.Key.StartsWith("exception.", StringComparison.Ordinal)))
          }
        );
      }
    };

    ActivitySource.AddActivityListener(ActivityListener);
  }

  public void Dispose() => ActivityListener.Dispose();
}
