namespace TestApp.Client.Features.Counter
{
  using MediatR;

  public class IncrementCounterAction : IRequest<CounterState>
  {
    public int Amount { get; set; }
  }
}