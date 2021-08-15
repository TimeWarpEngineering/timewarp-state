namespace TestApp.Client.Features.Base.Components
{
  using static TestApp.Client.Features.Application.ApplicationState;

  public partial class ResetButton : BaseComponent
  {
    internal void ButtonClick() => Mediator.Send(new ResetStoreAction());
  }
}