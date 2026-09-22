#region Purpose
// After each non-internal request, resets timers configured with ResetOnActivity.
#endregion

#region Design
// IInternalAction (including ResetTimersOnActivityActionSet.Action) skips the reset so the nested
// Sender.Send does not re-enter this behavior. Cached per closed generic.
// Do not inject TimerState; only Store.GetState assigns Sender. Direct send matches
// ActiveActionBehavior and forwards the request token.
#endregion

namespace TimeWarp.State.Plus.Features.Timers;

using static TimerState;

/// <summary>
/// Pipeline behavior that resets the activity timers after every non-internal request.
/// Opt-in: the host declares <c>[assembly: MediatorBehavior(typeof(MultiTimerPostProcessor&lt;,&gt;), order: ..., Scope = typeof(ClientPipeline))]</c>.
/// <see cref="IInternalAction"/> requests, including <c>ResetTimersOnActivityActionSet.Action</c>, skip the reset so the nested send does not recurse.
/// </summary>
public sealed class MultiTimerPostProcessor<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull
{
  private static readonly bool IsInternal = typeof(IInternalAction).IsAssignableFrom(typeof(TRequest));
  private readonly ILogger<MultiTimerPostProcessor<TRequest, TResponse>> Logger;
  private readonly ISender<ClientPipeline> Sender;

  public MultiTimerPostProcessor
  (
    ILogger<MultiTimerPostProcessor<TRequest, TResponse>> logger,
    ISender<ClientPipeline> sender
  )
  {
    Logger = logger;
    Sender = sender;
  }

  public async Task<TResponse> Handle
  (
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    TResponse response = await next(cancellationToken);
    if (IsInternal)
    {
      return response;
    }

    Logger.LogDebug(EventIds.MultiTimerPostProcessor_ProcessingRequest, message: "Processing request and checking timers");
    await Sender.Send(new ResetTimersOnActivityActionSet.Action(), cancellationToken);
    return response;
  }
}
