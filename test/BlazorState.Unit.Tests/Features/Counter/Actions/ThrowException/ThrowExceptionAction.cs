namespace BlazorState.Tests.Features.Counter
{
  using MediatR;

  public class ThrowExceptionAction : IRequest<CounterState>
  {
    public string Message { get; set; }
  }
}