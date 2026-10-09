#region Purpose
// ResetTimersOnActivity action set: restarts every timer configured with ResetOnActivity.
#endregion

#region Design
// An IInternalAction, so MultiTimerPostProcessor does not react to its own nested send and recurse.
#endregion

namespace TimeWarp.State.Plus.Features.Timers;

using System.Timers;
public partial class TimerState
{
  public static class ResetTimersOnActivityActionSet
  {
    public sealed class Action : IInternalAction;

    public sealed class Handler : StateActionHandler<Action>
    {
      private TimerState TimerState => Store.GetState<TimerState>();
      public Handler(IStore store) : base(store) { }

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        foreach ((string timerName, (Timer _, TimerConfig timerConfig)) in TimerState.Timers)
        {
          if (timerConfig.ResetOnActivity)
          {
            TimerState.RestartTimer(timerName);
          }
        }
        return default;
      }
    }
  }
}
