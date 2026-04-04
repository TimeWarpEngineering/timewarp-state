namespace TimeWarp.State.Plus.Features.Timers;

public partial class TimerState
{
  public static class AddTimerActionSet
  {
    internal sealed class Action : IAction
    {
      public string TimerName { get; }
      public TimerConfig TimerConfig { get; }
      public Action(string timerName, TimerConfig timerConfig)
      {
        TimerName = timerName;
        TimerConfig = timerConfig;
      }
    }

    internal sealed class Handler : ActionHandler<Action>
    {
      private TimerState TimerState => Store.GetState<TimerState>();
      public Handler(IStore store) : base(store) {}

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        TimerState.CreateAndStartTimer(action.TimerName, action.TimerConfig);
        return Task.CompletedTask;
      }
    }
  }
}
