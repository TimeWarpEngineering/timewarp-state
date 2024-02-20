namespace Test.App.Client.Features.Blue;

public partial class BlueState
{
  public static class IncrementCount
  {
    public class Action : IAction
    {
      public int Amount { get; init; }
    }

    public class Handler
    (
      IStore store
    ) : ActionHandler<Action>(store)
    {

      BlueState BlueState => Store.GetState<BlueState>();

      public override Task Handle(Action aAction, CancellationToken aCancellationToken)
      {
        BlueState.Count += aAction.Amount;
        return Task.CompletedTask;
      }
    }
  }
}
