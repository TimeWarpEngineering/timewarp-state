namespace TestApp.Client.Features.Counter.IncrementCount
{
  using MediatR;

  public class IncrementCounterAction : IRequest<CounterState>
  {
    public int Amount { get; set; }
  }
}