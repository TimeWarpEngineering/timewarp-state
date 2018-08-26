namespace BlazorState.Client.Features.Root.IncrementCount
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using BlazorStateSample.Client.Features.Counter.IncrementCount;
  using BlazorStateSample.Client.Features.Counter;

  public class IncrementCountHandler : RequestHandler<IncrementCountRequest, CounterState>
  {
    public IncrementCountHandler(IStore aStore) : base(aStore) { }

    public CounterState CounterState => Store.GetState<CounterState>();

    public override Task<CounterState> Handle(IncrementCountRequest aIncrementCountRequest, CancellationToken aCancellationToken)
    {
      CounterState.Count += aIncrementCountRequest.Amount;
      return Task.FromResult(CounterState);
    }
  }
}