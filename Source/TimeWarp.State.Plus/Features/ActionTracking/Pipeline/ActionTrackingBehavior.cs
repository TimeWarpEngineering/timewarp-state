namespace TimeWarp.Features.ActionTracking;

using static ActionTrackingState;

public class ActiveActionBehavior<TAction, TResponse>
(
  ISender sender
) : IPipelineBehavior<TAction, TResponse>
  where TAction : IAction
{
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
      
      await sender.Send(new StartProcessing.Action(action), cancellationToken);
      TResponse response = await nextHandler().ConfigureAwait(false);
      await sender.Send(new CompleteProcessing.Action(action), cancellationToken);
      return response;
    }
    else
    {
      TResponse response = await nextHandler().ConfigureAwait(false);
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
