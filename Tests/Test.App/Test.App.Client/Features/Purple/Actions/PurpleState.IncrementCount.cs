namespace Test.App.Client.Features.Purple;

public partial class PurpleState
{
  public static class IncrementCount
  {
    public class Action : IAction
    {
      public int Amount { get; init; }
    }

    [UsedImplicitly]
    public class Handler
    (
      IStore store
    ) : ActionHandler<Action>(store)
    {
      PurpleState PurpleState => Store.GetState<PurpleState>();

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        PurpleState.Count += action.Amount;
        return Task.CompletedTask;
      }
    }
  }
}
