namespace BlazorState.Pipeline.State
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using MediatR.Pipeline;
  using Microsoft.Extensions.Logging;

  internal class RenderSubscriptionsPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
  {
    public RenderSubscriptionsPostProcessor(
      ILogger<RenderSubscriptionsPostProcessor<TRequest, TResponse>> aLogger,
      Subscriptions aSubscriptions)
    {
      Logger = aLogger;
      Logger.LogDebug($"{GetType().Name}: constructor with TRequest:{typeof(TRequest).Name} TResponse:{typeof(TResponse).Name}");
      Subscriptions = aSubscriptions;
    }

    private ILogger Logger { get; }
    private Subscriptions Subscriptions { get; }

    public Task Process(TRequest aRequest, TResponse aResponse, CancellationToken aCancellationToken)
    {
      // logging variables
      string className = GetType().Name;
      className = className.Remove(className.IndexOf('`'));

      Logger.LogDebug($"{className}: Start");

      try
      {
        Logger.LogDebug($"{className}: ReRenderSubscribers");
        Subscriptions.ReRenderSubscribers<TResponse>();
        Logger.LogDebug($"{className}: End Post Processing");
      }
      catch (Exception aException)
      {
        Logger.LogError($"{className}: Error: {aException.Message}");
        Logger.LogError($"{className}: InnerError: {aException?.InnerException?.Message}");
        throw;
      }
      return Task.CompletedTask;
    }
  }
}