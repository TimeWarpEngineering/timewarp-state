namespace Test.App.Client.Pages;

using Microsoft.AspNetCore.Components;
using Test.App.Client.Features.Counter;
using System.Threading.Tasks;
using Test.App.Client.Features.Counter;
using Test.App.Client.Features.Counter2;

public partial class CounterOnePage
{
  [Inject] private IStore Store { get; set; } = null!;

  CounterOneState CounterOneState => GetState<CounterOneState>();
  Counter2State Counter2State => GetState<Counter2State>();

  async Task IncrementCount() => await Mediator.Send(new CounterOneState.IncrementCount.Action { Amount = 5 });
  async Task IncrementCount2() => await Mediator.Send(new Counter2State.IncrementCount.Action { Amount = 3 });
}
