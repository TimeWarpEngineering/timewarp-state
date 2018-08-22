namespace BlazorState.Client.Pages
{
  using BlazorState.Client.Components;

  public class CounterPageModel : BaseComponent
  {
    internal void ButtonClick() =>
      Mediator.Send(new Features.Counter.IncrementCount.IncrementCounterRequest { Amount = 5 });
  }
}
