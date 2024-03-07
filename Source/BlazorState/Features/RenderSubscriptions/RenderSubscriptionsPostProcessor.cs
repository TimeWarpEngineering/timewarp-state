# nullable enable

namespace TimeWarp.Features.RenderSubscriptions;

internal class RenderSubscriptionsPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
  where TRequest : notnull, IAction
{
  private readonly ILogger Logger;

  private readonly Subscriptions Subscriptions;

  public RenderSubscriptionsPostProcessor
  (
    ILogger<RenderSubscriptionsPostProcessor<TRequest, TResponse>> logger,
    Subscriptions subscriptions
  )
  {
    Logger = logger;
    Subscriptions = subscriptions;
  }

  public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
  {
    Type requestType = typeof(TRequest);
    Type enclosingStateType = requestType.GetEnclosingStateType();

    try
    {
      Subscriptions.ReRenderSubscribers(enclosingStateType);
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
