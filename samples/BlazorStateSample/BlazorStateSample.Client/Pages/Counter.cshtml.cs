using BlazorState;
using BlazorStateSample.Client.Features.Counter.IncrementCount;
using BlazorStateSample.Client.Features.Counter;
using MediatR;
using Microsoft.AspNetCore.Blazor.Components;

namespace BlazorStateSample.Client.Pages
{
  # region CounterModel

  public class CounterModel : BlazorComponent, IBlazorStateComponent
  {
    public CounterState CounterState => Store.GetState<CounterState>();
    [Inject] public IMediator Mediator { get; set; }
    [Inject] public IStore Store { get; set; }

    #region IncrementCount

    public void IncrementCount()
    {
      var incrementCountRequest = new IncrementCountRequest { Amount = 3 };
      Mediator.Send(incrementCountRequest);
    }

    #endregion IncrementCount
  }

  #endregion
}