namespace TimeWarp.State.Plus.Features.Timers;

public partial class TimerState
{
  public static class ResetTimersOnActivityActionSet
  {
    internal sealed class Action : IAction
    {
      // No additional properties needed for this action
    }

    internal sealed class Handler : ActionHandler<Action>
    {
      private TimerState TimerState => Store.GetState<TimerState>();
      public Handler(IStore store) : base(store) { }

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        foreach ((string timerName, (Timer _, TimerConfig timerConfig)) in TimerState.Timers)
        {
          if (timerConfig.ResetOnActivity)
          {
            TimerState.RestartTimer(timerName);
          }
        }
        return Task.CompletedTask;
      }
    }
  }
}
