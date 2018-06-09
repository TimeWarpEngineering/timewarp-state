namespace BlazorState.Client.Features.Root.IncrementCount
{
  using BlazorState.Client.State;
  using MediatR;

  public class Request : IRequest<CounterState>
  {
    public int Amount { get; set; }
  }
}