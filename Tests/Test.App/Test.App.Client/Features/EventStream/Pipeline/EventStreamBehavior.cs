namespace Test.App.Client.Features.EventStream;

using static EventStreamState;

/// <summary>
/// Every event that comes through the pipeline adds an object to the EventStreamState
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <remarks>To avoid infinite recursion don't add AddEvent to the event stream</remarks>
public class EventStreamBehavior<TRequest, TResponse>
(
  ILogger<EventStreamBehavior<TRequest, TResponse>> logger,
  ISender Sender
) : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IAction
{
  private readonly ILogger Logger = logger;

  public async Task<TResponse> Handle
  (
    TRequest aRequest,
    RequestHandlerDelegate<TResponse> aNext,
    CancellationToken aCancellationToken
  )
  {
    Logger.LogDebug("{classname}: Handle", GetType().Name);
    ArgumentNullException.ThrowIfNull(aNext);
    await AddEventToStream(aRequest, "Start");
    TResponse response = await aNext();
    await AddEventToStream(aRequest, "Completed");
    return response;
  }

  private async Task AddEventToStream(TRequest aRequest, string aTag)
  {
    if (aRequest is not AddEventAction)//Skip to avoid recursion
    {
      string requestTypeName = aRequest.GetType().Name;
      var addEventAction = new AddEventAction
      {
        Message = $"{aTag}:{requestTypeName}"
      };
      await Sender.Send(addEventAction);
    }
  }
}
