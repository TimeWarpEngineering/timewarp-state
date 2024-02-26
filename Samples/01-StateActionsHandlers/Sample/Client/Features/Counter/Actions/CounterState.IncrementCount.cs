namespace Sample.Client.Features.Counter;

[UsedImplicitly]
public partial class CounterState
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
      CounterState CounterState => Store.GetState<CounterState>();

      public override Task Handle(Action aAction, CancellationToken aCancellationToken)
      {
        CounterState.Count += aAction.Amount;
        return Task.CompletedTask;
      }
    }
  }
}
