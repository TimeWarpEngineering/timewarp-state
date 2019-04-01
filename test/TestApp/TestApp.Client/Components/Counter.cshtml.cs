namespace TestApp.Client.Components
{
  using TestApp.Client.Features.Counter;

  public class CounterModel : BaseComponent
  {
    internal void ButtonClick() =>
      Mediator.Send(new IncrementCounterAction { Amount = 5 });
  }
}