namespace BlazorStateSample.Client.Features.Counter.IncrementCount
{
  using BlazorStateSample.Client.Features.Counter.State;
  using MediatR;

  public class IncrementCountRequest : IRequest<CounterState>
  {
    public int Amount { get; set; }
  }
}