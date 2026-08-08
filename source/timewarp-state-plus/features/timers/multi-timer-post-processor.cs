namespace TimeWarp.State.Plus.Features.Timers;

public sealed class MultiTimerPostProcessor<TRequest, TResponse> : MessagePostProcessor<TRequest, TResponse>
  where TRequest : IMessage
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

  protected override async ValueTask Handle(TRequest request, TResponse response, CancellationToken cancellationToken)
  {
    Logger.LogDebug(EventIds.MultiTimerPostProcessor_ProcessingRequest, message: "Processing request and checking timers");
    await TimerState.ResetTimersOnActivity();
  }
}
