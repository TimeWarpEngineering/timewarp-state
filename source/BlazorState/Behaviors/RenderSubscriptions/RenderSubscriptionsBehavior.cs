namespace BlazorState.Behaviors.State
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;

  internal class RenderSubscriptionsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  {
    public RenderSubscriptionsBehavior(
      ILogger<CloneStateBehavior<TRequest, TResponse>> aLogger,
      IStore aStore,
      Subscriptions aSubscriptions)
    {
      Logger = aLogger;
      Logger.LogDebug($"{GetType().Name} constructor");
      Store = aStore;
      Subscriptions = aSubscriptions;
    }

    private ILogger Logger { get; }
    private IStore Store { get; }
    private Subscriptions Subscriptions { get; }

    public async Task<TResponse> Handle(
      TRequest aRequest,
      CancellationToken aCancellationToken,
      RequestHandlerDelegate<TResponse> aNext)
    {
      // logging variables
      string className = GetType().Name;
      className = className.Remove(className.IndexOf('`'));

      Logger.LogDebug($"{className}: Start");

      try
      {
        Logger.LogDebug($"{className}: Call next");
        TResponse response = await aNext();
        Logger.LogDebug($"{className}: Start Post Processing");
        Logger.LogDebug($"{className}: ReRenderSubscribers");
        Subscriptions.ReRenderSubscribers<TResponse>();
        Logger.LogDebug($"{className}: End Post Processing");
        return response;
      }
      catch (Exception aException)
      {
        Logger.LogError($"{className}: Error: {aException.Message}");
        Logger.LogError($"{className}: InnerError: {aException?.InnerException?.Message}");
        throw;  // Do you throw or not? for now yes.
      }
    }
  }
}