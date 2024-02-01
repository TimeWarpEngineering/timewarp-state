namespace Test.App.Client.Pages;

using BlazorState.Features.Routing;
using System.Threading.Tasks;
using Test.App.Client.Features.Base.Components;
using static Test.App.Client.Features.Counter.CounterState;

public partial class CounterPage : BaseComponent
{
  protected async Task ChangeRouteToHome() =>
    await Mediator.Send(new RouteState.ChangeRouteAction { NewRoute = "/" });

  protected async Task GoBack() =>
      await Mediator.Send(new RouteState.GoBackAction());

  protected async Task SendThrowExceptionAction() =>
    await Mediator.Send(new ThrowExceptionAction());

  protected async Task SendThrowServerSideExceptionAction() =>
    await Mediator.Send(new ThrowServerSideExceptionAction());
}
