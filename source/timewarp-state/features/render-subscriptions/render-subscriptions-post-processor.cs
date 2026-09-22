#region Purpose
// After an action is handled, re-render components subscribed to the enclosing state unless suppressed.
#endregion

#region Design
// [SuppressRender] is cached per closed generic. Distinct from IInternalAction: Start/Complete
// processing must still re-render ActionTracking UI.
// RenderSubscriptionContext holds only the in-flight action instance (reference equality).
// CompleteDispatch runs in finally around the whole Handle, including when next() throws, so a
// flag cannot leak to a later send of the same instance or pin it for the scoped lifetime.
#endregion

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
  private static readonly bool SuppressRender =
    typeof(TRequest).IsDefined(typeof(SuppressRenderAttribute), inherit: true);

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
    try
    {
      TResponse response = await next(cancellationToken);

      Type requestType = typeof(TRequest);
      Type enclosingStateType = requestType.GetEnclosingStateType();

      try
      {
        if (SuppressRender || !RenderSubscriptionContext.ShouldFireSubscriptionsForAction(request))
        {
          Logger.LogDebug
          (
            EventIds.RenderSubscriptionsPostProcessor_SkippedReRender,
            "Skipped re-rendering subscribers for action: {ActionType}",
            requestType.FullName
          );
        }
        else
        {
          Subscriptions.ReRenderSubscribers(enclosingStateType);
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
    finally
    {
      RenderSubscriptionContext.CompleteDispatch(request);
    }
  }
}
