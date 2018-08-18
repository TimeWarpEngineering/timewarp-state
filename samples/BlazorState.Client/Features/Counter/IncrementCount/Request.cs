namespace BlazorState.Client.Features.Counter.IncrementCount
{
  using MediatR;

  public class Request : IRequest<CounterState>
  {
    public int Amount { get; set; }
  }
}