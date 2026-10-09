#region Purpose
// StartProcessing action set: adds an action to ActionTrackingState's active list.
#endregion

#region Design
// An IInternalAction so tracking skips its own bookkeeping sends. Called by ActionTrackingBehavior around tracked
// actions.
#endregion

namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState
{
  public static class StartProcessingActionSet
  {
    public sealed class Action : IInternalAction
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
