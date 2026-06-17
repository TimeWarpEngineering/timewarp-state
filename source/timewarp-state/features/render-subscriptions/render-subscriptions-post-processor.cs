namespace TimeWarp.Features.RenderSubscriptions;

internal sealed class RenderSubscriptionsPostProcessor<TRequest, TResponse> : MessagePostProcessor<TRequest, TResponse>
  where TRequest : IAction
{
  private readonly ILogger Logger;
  private readonly Subscriptions Subscriptions;
  private readonly RenderSubscriptionContext RenderSubscriptionContext;

  public RenderSubscriptionsPostProcessor
  (
    ILogger<RenderSubscriptionsPostProcessor<TRequest, TResponse>> logger,
    Subscriptions subscriptions,
    RenderSubscriptionContext renderSubscriptionContext
  )
  {
    Logger = logger;
    Subscriptions = subscriptions;
    RenderSubscriptionContext = renderSubscriptionContext;
  }

  protected override ValueTask Handle(TRequest request, TResponse response, CancellationToken cancellationToken)
  {
    Type requestType = typeof(TRequest);
    Type enclosingStateType = requestType.GetEnclosingStateType();

    try
    {
      if (RenderSubscriptionContext.ShouldFireSubscriptionsForAction(request))
      {
        Subscriptions.ReRenderSubscribers(enclosingStateType);
      }
      else
      {
        Logger.LogDebug
        (
          EventIds.RenderSubscriptionsPostProcessor_SkippedReRender,
          "Skipped re-rendering subscribers for action: {ActionType}", 
          requestType.FullName
        );
      }
    }
    catch (Exception exception)
    {
      Logger.LogDebug
      (
        EventIds.RenderSubscriptionsPostProcessor_Exception,
        exception,
        "Error re-rendering subscriptions"
      );
      throw;
    }
    return default;
  }
}
