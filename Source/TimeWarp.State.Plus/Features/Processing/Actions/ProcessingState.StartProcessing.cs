namespace TimeWarp.Features.Processing;

public partial class ProcessingState
{
  public static class StartProcessing
  {
    internal record Action(string ActionName) : IAction;

    [UsedImplicitly]
    internal class Handler
    (
      IStore store
    ): ActionHandler<Action>(store)
    {
      private ProcessingState ProcessingState => Store.GetState<ProcessingState>();

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        ProcessingState.ActiveActionsList.Add(action.ActionName);
        return Task.CompletedTask;
      }
    }
  }
}
