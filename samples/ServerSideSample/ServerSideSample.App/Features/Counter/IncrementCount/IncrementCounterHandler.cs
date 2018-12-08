namespace ServerSideSample.App.Features.Counter
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using ServerSideSample.App.Features.Base;
  using ServerSideSample.App.Features.Counter.IncrementCount;

  public partial class CounterState
  {
    public class IncrementCounterHandler : BaseHandler<IncrementCounterRequest, CounterState>
    {
      public IncrementCounterHandler(IStore aStore) : base(aStore) { }

      public override Task<CounterState> Handle(
        IncrementCounterRequest aIncrementCounterRequest,
        CancellationToken aCancellationToken)
      {
        CounterState.Count += aIncrementCounterRequest.Amount;
        return Task.FromResult(CounterState);
      }
    }
  }
}