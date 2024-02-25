namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  [UsedImplicitly]
  public static class IncrementCount
  {
    
    [UsedImplicitly]
    public class Action : IAction
    {
      public int Amount { get; init; }
    }

    internal class Handler
    (
      IStore store
    ) : ActionHandler<Action>(store)
    {
      private CounterState CounterState => Store.GetState<CounterState>();
      
      [UsedImplicitly]
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
