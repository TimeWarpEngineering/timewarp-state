namespace TestApp.Client.Features.Counter.Components
{
  using TestApp.Client.Features.Base.Components;
  using TestApp.Client.Features.Counter;

  public class CounterBase : BaseComponent
  {
    internal void ButtonClick() =>
      Mediator.Send(new IncrementCounterAction { Amount = 5 });
  }
}