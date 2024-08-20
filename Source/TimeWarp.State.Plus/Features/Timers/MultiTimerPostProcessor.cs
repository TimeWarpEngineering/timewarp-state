namespace TimeWarp.State.Plus.Features.Timers;

public class MultiTimerPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
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

  public async Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
  {
    Logger.LogDebug(EventIds.MultiTimerPostProcessor_ProcessingRequest, message: "Processing request and checking timers");
    await TimerState.ResetTimersOnActivity();
  }
}
