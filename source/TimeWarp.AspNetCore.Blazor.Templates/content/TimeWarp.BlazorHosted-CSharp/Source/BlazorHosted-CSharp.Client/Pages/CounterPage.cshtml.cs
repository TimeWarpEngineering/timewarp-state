namespace BlazorHosted_CSharp.Client.Pages
{
  using BlazorHosted_CSharp.Client.Components;

  public class CounterPageModel : BaseComponent
  {
    internal void ButtonClick() =>
      Mediator.Send(new Features.Counter.IncrementCount.IncrementCounterAction { Amount = 5 });
  }
}