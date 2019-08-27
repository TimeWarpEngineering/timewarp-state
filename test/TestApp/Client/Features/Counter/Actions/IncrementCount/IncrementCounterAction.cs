namespace TestApp.Client.Features.Counter
{
  using MediatR;
  using TestApp.Shared.Features.Base;

  public class IncrementCounterAction : BaseRequest, IRequest<CounterState>
  {
    public int Amount { get; set; }
  }
}