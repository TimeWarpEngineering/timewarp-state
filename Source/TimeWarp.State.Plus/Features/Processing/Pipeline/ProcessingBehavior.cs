namespace TimeWarp.Features.Processing;

using static ProcessingState;

public class ProcessingBehavior<TRequest, TResponse>
(
  ISender sender
) : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IAction
{
  public async Task<TResponse> Handle
  (
    TRequest aRequest,
    RequestHandlerDelegate<TResponse> aNextHandler,
    CancellationToken aCancellationToken
  )
  {
    if (typeof(TRequest).GetCustomAttributes(typeof(TrackProcessingAttribute), false).Length != 0)
    {
      ArgumentValidation.EnsureNotType<TRequest, StartProcessing.Action>(aRequest, nameof(aRequest));
      ArgumentValidation.EnsureNotType<TRequest, CompleteProcessing.Action>(aRequest, nameof(aRequest));

      string actionName = typeof(TRequest).FullName!;
      await sender.Send(new StartProcessing.Action(actionName), aCancellationToken);
      TResponse response = await aNextHandler().ConfigureAwait(false);
      await sender.Send(new CompleteProcessing.Action(actionName), aCancellationToken);
      return response;
    }
    else
    {
      TResponse response = await aNextHandler().ConfigureAwait(false);
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
