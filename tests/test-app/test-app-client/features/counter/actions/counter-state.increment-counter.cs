namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public static class IncrementCountActionSet
  {
    
    public sealed class Action : IAction
    {
      public int Amount { get; init; }
    }

    internal sealed class Handler
    (
      IStore store
    ) : StateActionHandler<Action>(store)
    {
      private CounterState CounterState => Store.GetState<CounterState>();

      public override ValueTask Handle
      (
        Action action,
        CancellationToken cancellationToken
      )
      {
        CounterState.Count += action.Amount;
        return default;
      }
    }
  }
}
