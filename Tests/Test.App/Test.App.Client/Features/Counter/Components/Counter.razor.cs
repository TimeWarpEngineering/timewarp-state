namespace Test.App.Client.Features.Counter.Components;

using static CounterState;

public partial class Counter : BaseComponent
{
  private async Task ButtonClick() =>
    await Mediator.Send(new IncrementCounterAction { Amount = 5 });
}
