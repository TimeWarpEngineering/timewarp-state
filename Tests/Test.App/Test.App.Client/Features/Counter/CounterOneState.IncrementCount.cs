namespace Test.App.Client.Features.Counter;

public partial class CounterOneState
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

      CounterOneState CounterState => Store.GetState<CounterOneState>();

      public override Task Handle(Action aAction, CancellationToken aCancellationToken)
      {
        CounterState.Count += aAction.Amount;
        return Task.CompletedTask;
      }
    }
  }
}
