namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState
{
  public static class StartProcessingActionSet
  {
    internal sealed class Action : IAction
    {
      public Action(IAction theAction) 
      {
        TheAction = theAction;
      }
      public IAction TheAction { get; }
    }

    internal sealed class Handler : ActionHandler<Action>
    {
      public Handler(IStore store) : base(store) {}
      private ActionTrackingState ActionTrackingState => Store.GetState<ActionTrackingState>();

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        ActionTrackingState.ActiveActionList.Add(action.TheAction);
        return Task.CompletedTask;
      }
    }
  }
  
  public async Task StartProcessing(IAction theAction, CancellationToken? externalCancellationToken = null)
  {
    using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
      ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
      : null;

    await Sender.Send
    (
      new ActionTrackingState.StartProcessingActionSet.Action(theAction),
      linkedCts?.Token ?? CancellationToken
    );
  }
}
