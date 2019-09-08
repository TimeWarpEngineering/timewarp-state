namespace TestApp.Client.Features.Counter.Components
{
  using System.Threading.Tasks;
  using TestApp.Client.Features.Base.Components;
  using static TestApp.Client.Features.Counter.CounterState;

  public class CounterBase : BaseComponent
  {
    protected async Task ButtonClick() =>
      _ = await Mediator.Send(new IncrementCounterAction { Amount = 5 });
  }
}