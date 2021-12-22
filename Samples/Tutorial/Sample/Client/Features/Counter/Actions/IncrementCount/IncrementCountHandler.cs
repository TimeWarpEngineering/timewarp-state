namespace Sample.Client.Features.Counter
{
  using BlazorState;
  using MediatR;
  using System.Threading;
  using System.Threading.Tasks;

  public partial class CounterState
  {
    public class IncrementCountHandler : ActionHandler<IncrementCountAction>
    {
      public IncrementCountHandler(IStore aStore) : base(aStore) { }

      CounterState CounterState => Store.GetState<CounterState>();

      public override Task<Unit> Handle(IncrementCountAction aIncrementCountAction, CancellationToken aCancellationToken)
      {
        CounterState.Count = CounterState.Count + aIncrementCountAction.Amount;
        return Unit.Task;
      }
    }
  }
}