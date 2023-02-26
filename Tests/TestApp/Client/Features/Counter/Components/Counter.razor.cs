namespace TestApp.Client.Features.Counter.Components;

using System.Threading.Tasks;
using TestApp.Client.Features.Base.Components;
using static TestApp.Client.Features.Counter.CounterState;

public partial class Counter : BaseComponent
{
  protected async Task ButtonClick() =>
    await Mediator.Send(new IncrementCounterAction { Amount = 5 });
}
