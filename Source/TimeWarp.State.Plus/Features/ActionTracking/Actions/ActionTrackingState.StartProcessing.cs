namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState
{
  public static class StartProcessing
  {
    internal class Action(IAction theAction) : IAction
    {
      public IAction TheAction { get; init; } = theAction;
    }

    [UsedImplicitly]
    internal class Handler
    (
      IStore store
    ): ActionHandler<Action>(store)
    {
      private ActionTrackingState ActionTrackingState => Store.GetState<ActionTrackingState>();

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        ActionTrackingState.ActiveActionList.Add(action.TheAction);
        return Task.CompletedTask;
      }
    }
  }
}
