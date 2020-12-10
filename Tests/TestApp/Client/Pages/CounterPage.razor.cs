namespace TestApp.Client.Pages
{
  using BlazorState.Features.Routing;
  using System.Threading.Tasks;
  using TestApp.Client.Features.Base.Components;
  using static TestApp.Client.Features.Counter.CounterState;

  public class CounterPageBase : BaseComponent
  {
    protected async Task ChangeRouteToHome() =>
      await Mediator.Send(new RouteState.ChangeRouteAction { NewRoute = "/" });

    protected async Task SendThrowExceptionAction() =>
      await Mediator.Send(new ThrowExceptionAction());
  }
}