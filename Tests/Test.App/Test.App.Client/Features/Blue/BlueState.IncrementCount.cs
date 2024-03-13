namespace Test.App.Client.Features.Blue;

public partial class BlueState
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

      BlueState BlueState => Store.GetState<BlueState>();

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        BlueState.Count += action.Amount;
        return Task.CompletedTask;
      }
    }
  }
}
