namespace Sample.Client.Features.Counter;

using MediatR;

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

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        CounterState.Count += action.Amount;
        return Task.CompletedTask;
      }
    }
  }
}
