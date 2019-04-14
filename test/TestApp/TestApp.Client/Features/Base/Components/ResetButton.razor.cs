namespace TestApp.Client.Features.Base.Components
{
  using TestApp.Client.Features.Application;

  public class ResetButtonModel : BaseComponent
  {
    internal void ButtonClick() => Mediator.Send(new ResetStoreAction());
  }
}