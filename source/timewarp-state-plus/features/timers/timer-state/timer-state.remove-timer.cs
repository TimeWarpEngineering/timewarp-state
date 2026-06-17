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

    public sealed class Handler : ActionHandler<Action>
    {
      private TimerState TimerState => Store.GetState<TimerState>();
      
      public Handler(IStore store) : base(store) { }

      public override ValueTask<Unit> Handle(Action action, CancellationToken cancellationToken)
      {
        if (TimerState.Timers.TryGetValue(action.TimerName, out (Timer Timer, TimerConfig TimerConfig) timerTuple))
        {
          timerTuple.Timer.Dispose();
          TimerState.Timers.Remove(action.TimerName);
        }
        return new ValueTask<Unit>(Unit.Value);
      }
    }
  }
}
