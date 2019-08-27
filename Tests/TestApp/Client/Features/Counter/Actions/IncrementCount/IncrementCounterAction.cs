namespace TestApp.Client.Features.Counter
{
  using MediatR;
  using TestApp.Api.Features.Base;

  public class IncrementCounterAction : BaseRequest, IRequest<CounterState>
  {
    public int Amount { get; set; }
  }
}