namespace Test.App.Client.Features.Counter.Components;

using System.Threading.Tasks;
using Test.App.Client.Features.Base.Components;
using static Test.App.Client.Features.Counter.CounterState;

public partial class CounterZero : BaseComponent
{
  protected async Task ButtonClick() =>
    await Mediator.Send(new CounterZeroState.IncrementCounterAction { Amount = 5 });
}
