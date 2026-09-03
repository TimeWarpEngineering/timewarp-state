namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState
{
  public static class StartProcessingActionSet
  {
    public sealed class Action : IAction
    {
      public Action(IAction theAction) 
      {
        TheAction = theAction;
      }
      public IAction TheAction { get; }
    }

    public sealed class Handler : StateActionHandler<Action>
    {
      public Handler(IStore store) : base(store) {}
      private ActionTrackingState ActionTrackingState => Store.GetState<ActionTrackingState>();

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        ActionTrackingState.ActiveActionList.Add(action.TheAction);
        return default;
      }
    }
  }
}
