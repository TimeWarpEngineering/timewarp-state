namespace Test.App.Client.Features.Base.Components;

using static ApplicationState;

public partial class ResetButton : BaseComponent
{
  private void ButtonClick() => Mediator.Send(new ResetStoreAction());
}
