namespace CounterSample.Client.Features.Counter.IncrementCount
{
  using CounterSample.Client.Features.Counter.State;
  using MediatR;

  public class IncrementCountRequest : IRequest<CounterState>
  {
    public int Amount { get; set; }
  }
}