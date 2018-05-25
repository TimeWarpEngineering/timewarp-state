using BlazorState.Components;
using BlazorState.Store;
using CounterSample.Client.Features.Counter.IncrementCount;
using CounterSample.Client.Features.Counter.State;
using MediatR;
using Microsoft.AspNetCore.Blazor.Components;

namespace CounterSample.Client.Pages
{
  public class CounterModel : BlazorComponent, IBlazorStateComponent
  {
    public CounterState CounterState => Store.GetState<CounterState>();
    [Inject] public IMediator Mediator { get; set; }
    [Inject] public IStore Store { get; set; }

    public void IncrementCount()
    {
      var incrementCountRequest = new IncrementCountRequest { Amount = 3 };
      Mediator.Send(incrementCountRequest);
    }
  }
}