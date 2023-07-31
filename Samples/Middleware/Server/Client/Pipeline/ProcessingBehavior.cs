using BlazorState;
using MediatR;
using Middleware.Client.Features.Application;
using static Middleware.Client.Features.Application.ApplicationState;

namespace Middleware.Client.Pipeline;

public class ProcessingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull, IAction
{
  private readonly ISender Sender;

    public ProcessingBehavior(ISender sender) 
    { 
        Sender = sender;
    }

  public async Task<TResponse> Handle
  (
    TRequest aRequest,
    RequestHandlerDelegate<TResponse> aNextHandler,
    CancellationToken aCancellationToken
  )
  {
    if (typeof(TRequest).GetCustomAttributes(typeof(TrackProcessingAttribute), false).Any())
    {
        string actionName = typeof(TRequest).Name;
        await Sender.Send(new StartProcessingAction { ActionName = actionName}, aCancellationToken).ConfigureAwait(false);

        Console.WriteLine($"Processing {actionName}");

        TResponse response = await aNextHandler().ConfigureAwait(false);
        await Sender.Send(new CompleteProcessingAction{ ActionName = actionName}, aCancellationToken).ConfigureAwait(false);
      
        return response;
    }
    else
    {
      TResponse response = await aNextHandler().ConfigureAwait(false);
      return response;
    }
  }
}
