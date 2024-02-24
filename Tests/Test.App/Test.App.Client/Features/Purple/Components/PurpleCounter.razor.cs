namespace Test.App.Client.Features.Purple.Components;

[UsedImplicitly]
public partial class PurpleCounter
{
  [Inject] private ILogger<PurpleCounter> Logger { get; set; } = null!;
  private PurpleState PurpleState => GetState<PurpleState>();
  private async Task IncrementCount() => await Mediator.Send(new PurpleState.IncrementCount.Action { Amount = 5 });

  protected override void OnAfterRender(bool firstRender)
  {
    Logger.LogInformation("{TypeName}:OnAfterRender", GetType().Name);
    base.OnAfterRender(firstRender);
  }
}
