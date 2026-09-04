namespace TimeWarp.State.Plus.Features.Timers;

using System.Timers;

public partial class TimerState
{
  public static class AddTimerActionSet
  {
    public sealed class Action : IAction
    {
      public string TimerName { get; }
      public TimerConfig TimerConfig { get; }
      public Action(string timerName, TimerConfig timerConfig)
      {
        TimerName = timerName;
        TimerConfig = timerConfig;
      }
    }

    public sealed class Handler : StateActionHandler<Action>
    {
      private TimerState TimerState => Store.GetState<TimerState>();
      public Handler(IStore store) : base(store) {}

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        TimerState.Timers[action.TimerName] = (new Timer(action.TimerConfig.Duration), action.TimerConfig);
        return default;
      }
    }
  }
}
