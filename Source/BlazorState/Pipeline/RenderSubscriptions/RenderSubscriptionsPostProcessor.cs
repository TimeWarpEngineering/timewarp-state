# nullable enable

namespace BlazorState.Pipeline.RenderSubscriptions;

internal class RenderSubscriptionsPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
  where TRequest : notnull, IAction
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
    Subscriptions = aSubscriptions;
  }

  public Task Process(TRequest aRequest, TResponse aResponse, CancellationToken aCancellationToken)
  {
    Type requestType = typeof(TRequest);
    Type? declaringType = requestType.DeclaringType;

    try
    {
      Subscriptions.ReRenderSubscribers(declaringType);
    }
    catch (Exception aException)
    {
      Logger.LogDebug
      (
        EventIds.RenderSubscriptionsPostProcessor_Exception,
        aException,
        "Error re-rendering subscriptions"
      );
      throw;
    }
    return Task.CompletedTask;
  }
}
