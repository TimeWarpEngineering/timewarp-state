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
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    Logger.LogDebug("{classname}: Handle", GetType().FullName);
    ArgumentNullException.ThrowIfNull(next);
    await AddEventToStream(request, "Start");
    TResponse response = await next();
    await AddEventToStream(request, "Completed");
    return response;
  }

  private async Task AddEventToStream(TRequest request, string tag)
  {
    if (request is not AddEventActionSet.Action)//Skip to avoid recursion
    {
      string requestTypeName = request.GetType().FullName ?? "Unknown";
      string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

      var addEventAction = new AddEventActionSet.Action(message: $"{timestamp} {tag}:{requestTypeName}");
      await Sender.Send(addEventAction);
    }
  }
}
