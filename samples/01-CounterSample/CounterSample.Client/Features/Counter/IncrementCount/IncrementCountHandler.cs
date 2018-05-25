namespace BlazorState.Client.Features.Root.IncrementCount
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState.Store;
  using CounterSample.Client.Features.Counter.IncrementCount;
  using CounterSample.Client.Features.Counter.State;

  public class IncrementCountHandler : Handlers.RequestHandler<IncrementCountRequest, CounterState>
  {
    public IncrementCountHandler(IStore aStore) : base(aStore) { }

    public CounterState CounterState => Store.GetState<CounterState>();

    public override Task<CounterState> Handle(IncrementCountRequest request, CancellationToken cancellationToken)
    {
      CounterState.Count += request.Amount;
      return Task.FromResult(CounterState);
    }
  }
}