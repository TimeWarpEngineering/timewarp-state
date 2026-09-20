namespace TimeWarp.State.Plus.Features.Timers;

public partial class TimerState
{
  public static class UpdateTimerActionSet
  {
    public sealed class Action : IAction
    {
      public string TimerName { get; }
      public TimerConfig NewTimerConfig { get; }

      public Action(string timerName, TimerConfig newTimerConfig)
      {
        TimerName = timerName;
        NewTimerConfig = newTimerConfig;
      }
    }

    public sealed class Handler : StateActionHandler<Action>
    {
      private TimerState TimerState => Store.GetState<TimerState>();
      
      public Handler(IStore store) : base(store) { }

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        if (TimerState.Timers.ContainsKey(action.TimerName))
        {
          TimerState.CreateTimer(action.TimerName, action.NewTimerConfig);
        }
        return default;
      }
    }
  }
}
