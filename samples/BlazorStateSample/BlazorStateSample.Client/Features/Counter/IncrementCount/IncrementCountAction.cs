namespace BlazorStateSample.Client.Features.Counter.IncrementCount
{
  using MediatR;

  public class IncrementCountAction : IRequest<CounterState>
  {
    public int Amount { get; set; }
  }
}
