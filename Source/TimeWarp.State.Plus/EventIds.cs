namespace TimeWarp.State.Plus;

internal class EventIds
{
  // Feature - ActionTracking
  // Pipeline - Middleware
  public static readonly EventId ActionTrackingBehavior_StartTracking = new(1000, nameof(ActionTrackingBehavior_StartTracking));
  public static readonly EventId ActionTrackingBehavior_StartProcessing = new(1001, nameof(ActionTrackingBehavior_StartProcessing));
  public static readonly EventId ActionTrackingBehavior_CompletedProcessing = new(1002, nameof(ActionTrackingBehavior_CompletedProcessing));
  public static readonly EventId ActionTrackingBehavior_CompletedTracking = new(1003, nameof(ActionTrackingBehavior_CompletedTracking));
  
  // Feature - PersistentStatePostProcessor
  public static readonly EventId PersistentStatePostProcessor_StartProcessing = new(1100, nameof(PersistentStatePostProcessor_StartProcessing));
  public static readonly EventId PersistentStatePostProcessor_SaveToSessionStorage = new(1101, nameof(PersistentStatePostProcessor_SaveToSessionStorage));
  public static readonly EventId PersistentStatePostProcessor_SaveToLocalStorage = new(1102, nameof(PersistentStatePostProcessor_SaveToLocalStorage));
  
  // Feature - StateInitializedNotificationHandler
  public static readonly EventId StateInitializedNotificationHandler_Handling = new(1200, nameof(StateInitializedNotificationHandler_Handling));
  
  // Feature - PersistenceService
  public static readonly EventId PersistenceService_LoadState = new(1300, nameof(PersistenceService_LoadState));
  public static readonly EventId PersistenceService_LoadState_SerializedState = new(1301, nameof(PersistenceService_LoadState_SerializedState));
  public static readonly EventId PersistenceService_LoadState_DeserializationError = new(1302, nameof(PersistenceService_LoadState_DeserializationError));
  
  // Feature - MultiTimerPostProcessor
  public static readonly EventId MultiTimerPostProcessor_TimerStarted = new(1400, nameof(MultiTimerPostProcessor_TimerStarted));
  public static readonly EventId MultiTimerPostProcessor_TimerElapsed = new(1401, nameof(MultiTimerPostProcessor_TimerElapsed));
  public static readonly EventId MultiTimerPostProcessor_TimerRestarted = new(1402, nameof(MultiTimerPostProcessor_TimerRestarted));
  public static readonly EventId MultiTimerPostProcessor_TimerRestartAttemptFailed = new(1403, nameof(MultiTimerPostProcessor_TimerRestartAttemptFailed));
  public static readonly EventId MultiTimerPostProcessor_ProcessingRequest = new(1404, nameof(MultiTimerPostProcessor_ProcessingRequest));
}
