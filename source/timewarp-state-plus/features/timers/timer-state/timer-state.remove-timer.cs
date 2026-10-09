#region Purpose
// RemoveTimer action set: stops, disposes and removes a named timer.
#endregion

#region Design
// Unknown timer names are ignored rather than treated as errors.
#endregion

namespace TimeWarp.State.Plus.Features.Timers;

using System.Timers;

public partial class TimerState
{
  public static class RemoveTimerActionSet
  {
    public sealed class Action : IAction
    {
      public string TimerName { get; }
      
      public Action(string timerName)
      {
        TimerName = timerName;
      }
    }

    public sealed class Handler : StateActionHandler<Action>
    {
      private TimerState TimerState => Store.GetState<TimerState>();
      
      public Handler(IStore store) : base(store) { }

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        if (TimerState.Timers.TryGetValue(action.TimerName, out (Timer Timer, TimerConfig TimerConfig) timerTuple))
        {
          TimerState.StopAndDispose(timerTuple.Timer);
          TimerState.Timers.Remove(action.TimerName);
        }
        return default;
      }
    }
  }
}
