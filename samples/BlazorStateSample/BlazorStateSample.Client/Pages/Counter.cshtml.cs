namespace BlazorStateSample.Client.Pages
{
  using BlazorState;
  using BlazorStateSample.Client.Features.Counter;
  using BlazorStateSample.Client.Features.Counter.IncrementCount;
  using MediatR;
  using Microsoft.AspNetCore.Components;

  #region DocsCounterModel
  public class CounterModel : ComponentBase, IBlazorStateComponent
  {
    public CounterState CounterState => Store.GetState<CounterState>();
    [Inject] public IMediator Mediator { get; set; }
    [Inject] public IStore Store { get; set; }

    public void ReRender() => StateHasChanged();
    #endregion DocsCounterModel

    #region IncrementCount
    public void IncrementCount()
    {
      var incrementCountRequest = new IncrementCountAction { Amount = 3 };
      Mediator.Send(incrementCountRequest);
    }
    #endregion IncrementCount
  }
}
