namespace TimeWarp.State.Plus.Features.Timers;

/// <summary>
/// Pipeline behavior that resets the activity timers after every request.
/// Opt-in: the host declares <c>[assembly: MediatorBehavior(typeof(MultiTimerPostProcessor&lt;,&gt;), Order = ...)]</c>.
/// </summary>
public sealed class MultiTimerPostProcessor<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull
{
  private readonly ILogger<MultiTimerPostProcessor<TRequest, TResponse>> Logger;
  private readonly TimerState TimerState;

  public MultiTimerPostProcessor
  (
    ILogger<MultiTimerPostProcessor<TRequest, TResponse>> logger,
    TimerState timerState
  )
  {
    Logger = logger;
    TimerState = timerState;
  }

  public async Task<TResponse> Handle
  (
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    TResponse response = await next(cancellationToken);
    Logger.LogDebug(EventIds.MultiTimerPostProcessor_ProcessingRequest, message: "Processing request and checking timers");
    await TimerState.ResetTimersOnActivity();
    return response;
  }
}
