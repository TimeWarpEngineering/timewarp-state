namespace TimeWarp.State.Plus.Features.Timers;

using System.Timers;
public partial class TimerState
{
  public static class ResetTimersOnActivityActionSet
  {
    public sealed class Action : IAction;

    public sealed class Handler : ActionHandler<Action>
    {
      private TimerState TimerState => Store.GetState<TimerState>();
      public Handler(IStore store) : base(store) { }

      public override ValueTask<Unit> Handle(Action action, CancellationToken cancellationToken)
      {
        foreach ((string timerName, (Timer _, TimerConfig timerConfig)) in TimerState.Timers)
        {
          if (timerConfig.ResetOnActivity)
          {
            TimerState.RestartTimer(timerName);
          }
        }
        return new ValueTask<Unit>(Unit.Value);
      }
    }
  }
}
