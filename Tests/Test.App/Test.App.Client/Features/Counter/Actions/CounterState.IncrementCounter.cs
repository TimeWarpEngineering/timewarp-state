namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public static class IncrementCountActionSet
  {
    
    internal sealed class Action : IAction
    {
      public int Amount { get; init; }
    }

    internal sealed class Handler
    (
      IStore store
    ) : ActionHandler<Action>(store)
    {
      private CounterState CounterState => Store.GetState<CounterState>();
      
      public override Task Handle
      (
        Action action,
        CancellationToken cancellationToken
      )
      {
        CounterState.Count += action.Amount;
        return Task.CompletedTask;
      }
    }
  }
}
