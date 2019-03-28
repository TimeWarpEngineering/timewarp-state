namespace TestApp.Client.Pages
{
  using TestApp.Client.Components;

  public class CounterPageModel : BaseComponent
  {
    internal void ButtonClick() =>
      Mediator.Send(new Features.Counter.IncrementCount.IncrementCounterAction { Amount = 5 });
  }
}