namespace TestApp.Client.Features.Base.Components
{
  using static TestApp.Client.Features.Application.ApplicationState;

  public class ResetButtonBase : BaseComponent
  {
    internal void ButtonClick() => Mediator.Send(new ResetStoreAction());
  }
}