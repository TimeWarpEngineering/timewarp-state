namespace BlazorState.Client.Features.Root.IncrementCount
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState.Client.Features.Base;
  using BlazorState.Client.State;
  using BlazorState.Store;

  public class Handler : BaseHandler<Request, CounterState>
  {
    public Handler(IStore aStore) : base(aStore) { }

    public override Task<CounterState> Handle(Request request, CancellationToken cancellationToken)
    {
      CounterState.Count += request.Amount;
      return Task.FromResult(CounterState);
    }
  }
}