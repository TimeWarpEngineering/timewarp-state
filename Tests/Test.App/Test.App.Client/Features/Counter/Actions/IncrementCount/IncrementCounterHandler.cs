namespace Test.App.Client.Features.Counter;

using Test.App.Client.Features.Base;

public partial class CounterZeroState
{
  internal class IncrementCounterHandler : BaseActionHandler<IncrementCounterAction>
  {
    public IncrementCounterHandler(IStore store) : base(store) { }

    public override Task Handle
    (
      IncrementCounterAction aIncrementCounterAction,
      CancellationToken aCancellationToken
    )
    {
      CounterZeroState.Count += aIncrementCounterAction.Amount;
      return Task.CompletedTask;
    }
  }
}
