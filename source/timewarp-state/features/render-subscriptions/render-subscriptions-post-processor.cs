namespace TimeWarp.Features.RenderSubscriptions;

/// <summary>
/// Pipeline behavior that re-renders the subscribers of the enclosing state after an action is handled.
/// Woven by <c>[assembly: MediatorBehavior]</c> in assembly-marker.cs; closes only onto <see cref="IAction"/> requests.
/// </summary>
/// <remarks>
/// Public (not internal): the consuming host's generated mediator resolves the closed behavior type by name.
/// </remarks>
public sealed class RenderSubscriptionsPostProcessor<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull, IAction
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

  public async Task<TResponse> Handle
  (
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    TResponse response = await next(cancellationToken);

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

    return response;
  }
}
