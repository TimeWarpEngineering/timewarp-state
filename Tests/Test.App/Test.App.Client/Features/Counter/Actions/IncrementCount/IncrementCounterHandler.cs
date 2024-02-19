namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  internal class IncrementCounterHandler
  (
    IStore store
  ) : BaseActionHandler<IncrementCounterAction>(store)
  {

    public override Task Handle
    (
      IncrementCounterAction aIncrementCounterAction,
      CancellationToken aCancellationToken
    )
    {
      CounterState.Count += aIncrementCounterAction.Amount;
      return Task.CompletedTask;
    }
  }
}
