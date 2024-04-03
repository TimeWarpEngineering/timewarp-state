namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState
{
  public static class CompleteProcessing
  {
    public record Action(IAction TheAction) : IAction;

    [UsedImplicitly]
    internal class Handler
    (
      IStore store,
      ILogger<Handler> Logger
    ): ActionHandler<Action>(store)
    {
      private ActionTrackingState ActionTrackingState => Store.GetState<ActionTrackingState>();
      
      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        bool wasRemoved = ActionTrackingState.ActiveActionList.Remove(action.TheAction);
        if (wasRemoved) return Task.CompletedTask;
        
        // I want to know if this ever happens
        Logger.LogDebug("Action {action} was not found in the active action list.", action.TheAction.GetType().FullName);
        
        string? activeActions = ActionTrackingState.ActiveActionList
          .Select(a => a.GetType().FullName)
          .Aggregate((current, next) => $"{current}, {next}");
        
        Logger.LogDebug("Active actions: {activeActions}", activeActions);
        
        throw new InvalidOperationException("Action was not found in the active action list.");
      }
    }
  }
}
