namespace Sample.Client.Features.Counter
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;

  public partial class CounterState
  {
    public class IncrementCounterHandler : RequestHandler<IncrementCounterRequest, CounterState>
    {
      public IncrementCounterHandler(IStore aStore) : base(aStore) { }

      public CounterState CounterState => Store.GetState<CounterState>();

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