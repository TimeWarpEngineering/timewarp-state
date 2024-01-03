namespace SampleDotNet8.Client.Pages;

using BlazorState;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SampleDotNet8.Client.Features.Counter;
using System;
using System.Threading.Tasks;

public partial class Counter : IDisposable
{
  [Inject] PersistentComponentState PersistentComponentState { get; set; }
  [Inject] private IStore Store { get; set; }

  private PersistingComponentStateSubscription PersistingComponentStateSubscription;
  CounterState CounterState => GetState<CounterState>();

  async Task IncrementCount()
  {
    await Mediator.Send(new CounterState.IncrementCount.Action { Amount = 5 });
  }

  protected override async Task OnInitializedAsync()
  {
    await base.OnInitializedAsync();
    PersistingComponentStateSubscription = PersistentComponentState.RegisterOnPersisting(OnPersisting);

    bool foundInState =
      PersistentComponentState.TryTakeFromJson<CounterState>(nameof(CounterState), out CounterState? counterState);

    if (foundInState)
    {
      Store.SetState(counterState);
    }
  }

  private Task OnPersisting()
  {
    PersistentComponentState.PersistAsJson(instance: CounterState, key: nameof(CounterState));
    return Task.CompletedTask;
  }

  void IDisposable.Dispose()
  {
    PersistingComponentStateSubscription.Dispose();
  }
}
