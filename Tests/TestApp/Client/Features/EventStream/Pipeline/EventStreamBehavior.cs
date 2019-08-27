namespace TestApp.Client.Features.EventStream
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;
  using TestApp.Api.Features.Base;

  /// <summary>
  /// Every event that comes through the pipeline adds an object to the EventStreamState
  /// </summary>
  /// <typeparam name="TRequest"></typeparam>
  /// <typeparam name="TResponse"></typeparam>
  /// <remarks>To avoid infinite recursion don't add AddEvent to the event stream</remarks>
  public class EventStreamBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  {
    public EventStreamBehavior
    (
      ILogger<EventStreamBehavior<TRequest, TResponse>> aLogger,
      IMediator aMediator
    )
    {
      Logger = aLogger;
      Mediator = aMediator;
      Logger.LogDebug($"{GetType().Name}: Constructor");
    }

    public Guid Guid { get; } = Guid.NewGuid();
    private ILogger Logger { get; }
    private IMediator Mediator { get; }

    public async Task<TResponse> Handle
    (
      TRequest aRequest,
      CancellationToken aCancellationToken,
      RequestHandlerDelegate<TResponse> aNext
    )
    {
      await AddEventToStream(aRequest, "Start");
      TResponse newState = await aNext();
      await AddEventToStream(aRequest, "Completed");
      return newState;
    }

    private async Task AddEventToStream(TRequest aRequest, string aTag)
    {
      if (!(aRequest is AddEventAction)) //Skip to avoid recursion
      {
        var addEventAction = new AddEventAction();
        string requestTypeName = aRequest.GetType().Name;

        if (aRequest is BaseRequest request)
        {
          addEventAction.Message = $"{aTag}:{requestTypeName}:{request.Id}";
        }
        else
        {
          addEventAction.Message = $"{aTag}:{requestTypeName}";
        }
        await Mediator.Send(addEventAction);
      }
    }
  }
}