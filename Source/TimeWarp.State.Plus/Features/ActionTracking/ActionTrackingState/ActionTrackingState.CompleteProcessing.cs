namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState
{
  public static class CompleteProcessingActionSet
  {
    internal sealed class Action : IAction
    {
      public IAction TheAction { get; }
      public Action(IAction theAction)
      {
        TheAction = theAction;
      }
    }

    [UsedImplicitly]
    internal sealed class Handler : ActionHandler<Action>
    {
      private readonly ILogger<Handler> Logger;
      public Handler(IStore store, ILogger<Handler> logger) : base(store)
      {
        Logger = logger;
      }
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
