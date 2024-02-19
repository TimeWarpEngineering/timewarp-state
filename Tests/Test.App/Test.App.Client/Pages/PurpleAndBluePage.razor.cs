namespace Test.App.Client.Pages;

using Microsoft.AspNetCore.Components;
using Test.App.Client.Features.Purple;
using Test.App.Client.Features.Counter2;

public partial class PurpleAndBluePage
{
  [Inject] private IStore Store { get; set; } = null!;

  PurpleState PurpleState => GetState<PurpleState>();
  BlueState BlueState => GetState<BlueState>();

  async Task IncrementPurpleCount() => await Mediator.Send(new PurpleState.IncrementCount.Action { Amount = 5 });
  async Task IncrementCount2() => await Mediator.Send(new BlueState.IncrementCount.Action { Amount = 3 });
}
