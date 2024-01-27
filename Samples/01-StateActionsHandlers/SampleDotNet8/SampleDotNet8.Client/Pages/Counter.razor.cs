namespace SampleDotNet8.Client.Pages;

using Microsoft.AspNetCore.Components;
using SampleDotNet8.Client.Features.Counter;
using System.Threading.Tasks;

public partial class Counter
{
  [Inject] private IStore Store { get; set; } = null!;

  CounterState CounterState => GetState<CounterState>();

  async Task IncrementCount() => await Mediator.Send(new CounterState.IncrementCount.Action { Amount = 5 });
}
