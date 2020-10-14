namespace TestApp.Client.Features.Base.Components
{
  using TestApp.Client.Features.Application;

  public class ResetButtonBase : BaseComponent
  {
    internal void ButtonClick() => Mediator.Send(new ResetStoreAction());
  }
}