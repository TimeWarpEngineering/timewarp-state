namespace BlazorState.Client.Features.Counter
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using BlazorState.Client.Features.Base;
  using BlazorState.Client.Features.Counter.IncrementCount;

  public partial class CounterState
  {
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
}