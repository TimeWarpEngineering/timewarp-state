namespace TimeWarp.State.Plus.Features.Timers;

using System.Timers;

public partial class TimerState
{
  public static class UpdateTimerActionSet
  {
    internal sealed class Action : IAction
    {
      public string TimerName { get; }
      public TimerConfig NewTimerConfig { get; }

      public Action(string timerName, TimerConfig newTimerConfig)
      {
        TimerName = timerName;
        NewTimerConfig = newTimerConfig;
      }
    }

    internal sealed class Handler : ActionHandler<Action>
    {
      private TimerState TimerState => Store.GetState<TimerState>();
      
      public Handler(IStore store) : base(store) { }

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        if (TimerState.Timers.TryGetValue(action.TimerName, out (Timer Timer, TimerConfig TimerConfig) timerTuple))
        {
          timerTuple.Timer.Dispose();
          Timer newTimer = new(action.NewTimerConfig.Duration);
          TimerState.Timers[action.TimerName] = (newTimer, action.NewTimerConfig);
        }
        return Task.CompletedTask;
      }
    }
  }
}
