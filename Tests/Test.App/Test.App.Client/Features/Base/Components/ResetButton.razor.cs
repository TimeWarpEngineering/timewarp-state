namespace Test.App.Client.Features.Base.Components;

using static Test.App.Client.Features.Application.ApplicationState;

public partial class ResetButton : BaseComponent
{
  internal void ButtonClick() => Mediator.Send(new ResetStoreAction());
}
