namespace ServerSideSample.App.Pages
{
  using ServerSideSample.App.Components;

  public class CounterPageModel : BaseComponent
  {
    internal void ButtonClick() =>
      Mediator.Send(new Features.Counter.IncrementCount.IncrementCounterRequest { Amount = 5 });
  }
}