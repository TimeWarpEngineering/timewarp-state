namespace TimeWarp.State.Plus;

internal class EventIds
{
  // Features - ActionTracking
  // Pipeline - Middleware
  public static readonly EventId ActionTrackingBehavior_StartTracking = new(1000, nameof(ActionTrackingBehavior_StartTracking));
  public static readonly EventId ActionTrackingBehavior_StartProcessing = new(1001, nameof(ActionTrackingBehavior_StartProcessing));
  public static readonly EventId ActionTrackingBehavior_CompletedProcessing = new(1002, nameof(ActionTrackingBehavior_CompletedProcessing));
  public static readonly EventId ActionTrackingBehavior_CompletedTracking = new(1003, nameof(ActionTrackingBehavior_CompletedTracking));
}
