namespace Test.App.Client.Features.Counter2;

public partial class Counter2State
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

      Counter2State Counter2State => Store.GetState<Counter2State>();

      public override Task Handle(Action aAction, CancellationToken aCancellationToken)
      {
        Counter2State.Count += aAction.Amount;
        return Task.CompletedTask;
      }
    }
  }
}
