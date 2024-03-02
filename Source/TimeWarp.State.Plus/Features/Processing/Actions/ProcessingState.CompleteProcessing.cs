namespace TimeWarp.Features.Processing;

public partial class ProcessingState
{
  public static class CompleteProcessing
  {
    public record Action(string ActionName) : IAction;

    [UsedImplicitly]
    internal class Handler
    (
      IStore store
    ): ActionHandler<Action>(store)
    {
      private ProcessingState ProcessingState => Store.GetState<ProcessingState>();
      
      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        ProcessingState.ActiveActionsList.Remove(action.ActionName);
        return Task.CompletedTask;
      }
    }
  }
}
