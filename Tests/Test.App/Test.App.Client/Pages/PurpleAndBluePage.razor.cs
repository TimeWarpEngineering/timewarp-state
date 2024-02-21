namespace Test.App.Client.Pages;

// ReSharper disable once ClassNeverInstantiated.Global
public partial class PurpleAndBluePage
{
  [Inject] private IStore Store { get; set; } = null!;

  PurpleState PurpleState => GetState<PurpleState>();
  BlueState BlueState => GetState<BlueState>();

  async Task IncrementPurpleCount() => await Mediator.Send(new PurpleState.IncrementCount.Action { Amount = 5 });
  async Task IncrementBlueCount() => await Mediator.Send(new BlueState.IncrementCount.Action { Amount = 3 });
}
