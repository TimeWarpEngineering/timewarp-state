namespace Test.App.Client.Features.Counter;

using System.Threading;
using System.Threading.Tasks;
using Test.App.Client.Features.Base;

public partial class CounterState
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
      CounterState.Count += aIncrementCounterAction.Amount;
      return Task.CompletedTask;
    }
  }
}
