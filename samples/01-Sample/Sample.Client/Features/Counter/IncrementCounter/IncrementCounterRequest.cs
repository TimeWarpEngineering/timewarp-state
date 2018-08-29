namespace Sample.Client.Features.Counter
{
  using MediatR;

  public class IncrementCounterRequest : IRequest<CounterState>
  {
    public int Amount { get; set; }
  }
}
