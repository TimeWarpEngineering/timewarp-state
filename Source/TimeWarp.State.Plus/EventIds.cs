namespace TimeWarp.State.Plus;

internal class EventIds
{
  // Features - ActionTracking
  // Pipeline - Middleware
  public static readonly EventId ActionTrackingBehavior_StartTracking = new(700, nameof(ActionTrackingBehavior_StartTracking));
  public static readonly EventId ActionTrackingBehavior_StartProcessing = new(701, nameof(ActionTrackingBehavior_StartProcessing));
  public static readonly EventId ActionTrackingBehavior_CompletedProcessing = new(702, nameof(ActionTrackingBehavior_CompletedProcessing));
  public static readonly EventId ActionTrackingBehavior_CompletedTracking = new(703, nameof(ActionTrackingBehavior_CompletedTracking));
}
