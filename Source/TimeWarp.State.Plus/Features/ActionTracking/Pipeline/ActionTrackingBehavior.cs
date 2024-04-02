namespace TimeWarp.Features.ActionTracking;

using static ActionTrackingState;

public class ActiveActionBehavior<TAction, TResponse>
(
  ISender Sender,
  ILogger<ActiveActionBehavior<TAction, TResponse>> logger
) : IPipelineBehavior<TAction, TResponse>
  where TAction : IAction
{
  private readonly ILogger Logger = logger;
  
  public async Task<TResponse> Handle
  (
    TAction action,
    RequestHandlerDelegate<TResponse> nextHandler,
    CancellationToken cancellationToken
  )
  {
    if (typeof(TAction).GetCustomAttributes(typeof(TrackActionAttribute), false).Length != 0)
    {
      ArgumentValidation.EnsureNotType<TAction, StartProcessing.Action>(action, nameof(action));
      ArgumentValidation.EnsureNotType<TAction, CompleteProcessing.Action>(action, nameof(action));

      Logger.LogDebug
      (
        State.Plus.EventIds.ActionTrackingBehavior_StartTracking,
        "Start tracking Action of type {actionType}",
        action.GetType().FullName
      );
      await Sender.Send(new StartProcessing.Action(action), cancellationToken);
      
      Logger.LogDebug
      (
        State.Plus.EventIds.ActionTrackingBehavior_StartProcessing,
        "Start processing Action of type {actionType}",
        action.GetType().FullName
      );
      
      TResponse response = await nextHandler();
      
      Logger.LogDebug
      (
        State.Plus.EventIds.ActionTrackingBehavior_CompletedProcessing,
        "Completed process Action of type {actionType}",
        action.GetType().FullName
      );
      
      
      
      await Sender.Send(new CompleteProcessing.Action(action), cancellationToken);
      Logger.LogDebug
      (
        State.Plus.EventIds.ActionTrackingBehavior_CompletedTracking,
        "Completed tracking Action of type {actionType}",
        action.GetType().FullName
      );
      return response;
    }
    else
    { 
      TResponse response = await nextHandler();
      return response;
    }
  }
}

public static class ArgumentValidation
{
  public static void EnsureNotType<TArgument, TInvalidType>(TArgument argument, string argumentName)
    where TInvalidType : class
  {
    if (argument is TInvalidType)
    {
      throw new ArgumentException($"Argument {argumentName} must not be of type {typeof(TInvalidType).Name}.", argumentName);
    }
  }
}
