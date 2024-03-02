namespace TimeWarp.Features.Processing;

public partial class ActiveActionState
{
  public static class StartProcessing
  {
    internal record Action(IAction TheAction) : IAction;

    [UsedImplicitly]
    internal class Handler
    (
      IStore store
    ): ActionHandler<Action>(store)
    {
      private ActiveActionState ActiveActionState => Store.GetState<ActiveActionState>();

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        ActiveActionState.ActiveActionsList.Add(action.TheAction);
        return Task.CompletedTask;
      }
    }
  }
}
