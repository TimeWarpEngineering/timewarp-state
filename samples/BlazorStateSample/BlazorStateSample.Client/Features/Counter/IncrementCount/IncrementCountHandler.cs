using BlazorState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorStateSample.Client.Features.Counter.IncrementCount
{
  public class IncrementCountHandler : RequestHandler<IncrementCountAction, CounterState>
  {
    public IncrementCountHandler(IStore aStore) : base(aStore) { }

    public CounterState CounterState => Store.GetState<CounterState>();

    public override Task<CounterState> Handle(IncrementCountAction aIncrementCountAction, CancellationToken aCancellationToken)
    {
      CounterState.Count += aIncrementCountAction.Amount;
      return Task.FromResult(CounterState);
    }
  }
}
