namespace Sample.Client.Pages
{
  using System;
  using BlazorState;
  using MediatR;
  using Microsoft.AspNetCore.Blazor.Components;
  using Sample.Client.Features.Counter;

  public class CounterModel : BlazorComponent, IBlazorStateComponent
  {
    public CounterState CounterState => Store.GetState<CounterState>();
    [Inject] public IMediator Mediator { get; set; }
    [Inject] public IStore Store { get; set; }

    // region used by docFx
    #region ButtonClick
    public void ButtonClick()
    {
      var incrementCounterRequest = new IncrementCounterRequest { Amount = 2 };
      Mediator.Send(incrementCounterRequest);
    }
    #endregion
  }
}
