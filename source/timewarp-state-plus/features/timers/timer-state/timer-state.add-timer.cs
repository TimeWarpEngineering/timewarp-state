#region Purpose
// AddTimer action set: creates a named timer in TimerState.
#endregion

#region Design
// The handler delegates to TimerState.CreateTimer so timer lifecycle logic stays in the state.
#endregion

namespace TimeWarp.State.Plus.Features.Timers;

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
        TimerState.CreateTimer(action.TimerName, action.TimerConfig);
        return default;
      }
    }
  }
}
