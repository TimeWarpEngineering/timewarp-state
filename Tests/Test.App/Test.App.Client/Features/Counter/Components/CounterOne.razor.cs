namespace Test.App.Client.Features.Counter.Components;

using Microsoft.AspNetCore.Components;

public partial class CounterOne
{
  [Inject] private IStore Store { get; set; } = null!;
  [Inject] private ILogger<CounterOne> Logger { get; set; } = null!;
  private CounterOneState CounterOneState => GetState<CounterOneState>();
  private async Task IncrementCount() => await Mediator.Send(new CounterOneState.IncrementCount.Action() { Amount = 5 });

  protected override void OnAfterRender(bool firstRender)
  {
    Logger.LogInformation($"{GetType().Name}:OnAfterRender");
    base.OnAfterRender(firstRender);
  }
}
