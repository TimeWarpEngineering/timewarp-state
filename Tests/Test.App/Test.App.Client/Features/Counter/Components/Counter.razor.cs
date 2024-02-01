namespace Test.App.Client.Features.Counter.Components;

using System.Threading.Tasks;
using Test.App.Client.Features.Base.Components;
using static Test.App.Client.Features.Counter.CounterState;

public partial class Counter : BaseComponent
{
  protected async Task ButtonClick() =>
    await Mediator.Send(new IncrementCounterAction { Amount = 5 });
}
