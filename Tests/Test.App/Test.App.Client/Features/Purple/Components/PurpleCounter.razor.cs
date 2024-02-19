namespace Test.App.Client.Features.Purple.Components;

using Microsoft.AspNetCore.Components;
using Purple;

public partial class PurpleCounter
{
  [Inject] private IStore Store { get; set; } = null!;
  [Inject] private ILogger<PurpleCounter> Logger { get; set; } = null!;
  private PurpleState PurpleState => GetState<PurpleState>();
  private async Task IncrementCount() => await Mediator.Send(new PurpleState.IncrementCount.Action() { Amount = 5 });

  protected override void OnAfterRender(bool firstRender)
  {
    Logger.LogInformation($"{GetType().Name}:OnAfterRender");
    base.OnAfterRender(firstRender);
  }
}
