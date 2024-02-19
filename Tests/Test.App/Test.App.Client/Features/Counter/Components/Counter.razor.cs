namespace Test.App.Client.Features.Counter.Components;

using static Test.App.Client.Features.Counter.CounterState;

public partial class Counter : BaseComponent
{
  protected async Task ButtonClick() =>
    await Mediator.Send(new CounterState.IncrementCounterAction { Amount = 5 });
}
