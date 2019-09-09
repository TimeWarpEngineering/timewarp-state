namespace BlazorState.Pipeline.State
{
  using BlazorState;
  using MediatR.Pipeline;
  using Microsoft.Extensions.Logging;
  using System;
  using System.Threading;
  using System.Threading.Tasks;

  internal class RenderSubscriptionsPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
  {
    private readonly ILogger Logger;

    private readonly Subscriptions Subscriptions;

    public RenderSubscriptionsPostProcessor
    (
      ILogger<RenderSubscriptionsPostProcessor<TRequest, TResponse>> aLogger,
      Subscriptions aSubscriptions
    )
    {
      Logger = aLogger;
      Logger.LogDebug($"{GetType().Name}: constructor with TRequest:{typeof(TRequest).Name} TResponse:{typeof(TResponse).Name}");
      Subscriptions = aSubscriptions;
    }

    public Task Process(TRequest aRequest, TResponse aResponse, CancellationToken aCancellationToken)
    {
      Type declaringType = typeof(TRequest).DeclaringType;

      // logging variables
      string className = GetType().Name;
      className = className.Remove(className.IndexOf('`'));

      Logger.LogDebug($"{className}: Start");

      try
      {
        Logger.LogDebug($"{className}: ReRenderSubscribers");
        Subscriptions.ReRenderSubscribers(declaringType);
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