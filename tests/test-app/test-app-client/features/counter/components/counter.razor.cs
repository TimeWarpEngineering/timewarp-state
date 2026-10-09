#region Purpose
// Code-behind for the Counter component: the button sends IncrementCount with Amount 5.
#endregion

#region Design
// Inherits BaseComponent, so reading CounterState subscribes the component and it re-renders when the count changes.
#endregion

namespace Test.App.Client.Features.Counter.Components;

using static CounterState;

public partial class Counter : BaseComponent
{
  private async Task ButtonClick() =>
    await Sender.Send(new IncrementCountActionSet.Action { Amount = 5 });
}
