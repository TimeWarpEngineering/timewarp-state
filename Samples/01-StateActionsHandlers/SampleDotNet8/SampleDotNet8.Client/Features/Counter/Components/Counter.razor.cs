namespace SampleDotNet8.Client.Features.Counter.Components;

using Microsoft.AspNetCore.Components;

public partial class Counter
{
  [Inject] private IStore Store { get; set; } = null!;
  [Inject] private ILogger<Counter> Logger { get; set; } = null!;
  private CounterState CounterState => GetState<CounterState>();
  private async Task IncrementCount() => await Mediator.Send(new CounterState.IncrementCount.Action { Amount = 5 });

  protected override void OnAfterRender(bool firstRender)
  {
    Logger.LogInformation($"{GetType().Name}:OnAfterRender");
    base.OnAfterRender(firstRender);
  }
}
