namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState
{
  public static class CompleteProcessing
  {
    public record Action(IAction TheAction) : IAction;

    [UsedImplicitly]
    internal class Handler
    (
      IStore store
    ): ActionHandler<Action>(store)
    {
      private ActionTrackingState ActionTrackingState => Store.GetState<ActionTrackingState>();
      
      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        ActionTrackingState.ActiveActionList.Remove(action.TheAction);
        return Task.CompletedTask;
      }
    }
  }
}
