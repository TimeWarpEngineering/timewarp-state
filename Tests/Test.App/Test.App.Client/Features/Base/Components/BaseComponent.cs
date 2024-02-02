namespace Test.App.Client.Features.Base.Components;

using BlazorState.Pipeline.ReduxDevTools;
using System.Threading.Tasks;
using Test.App.Client.Features.Application;
using Test.App.Client.Features.Counter;
using Test.App.Client.Features.EventStream;
using Test.App.Client.Features.WeatherForecast;

/// <summary>
/// Makes access to the State a little easier and by inheriting from
/// BlazorStateDevToolsComponent it allows for ReduxDevTools operation.
/// </summary>
/// <remarks>
/// In production one would NOT be required to use these base components
/// But would be required to properly implement the required interfaces.
/// one could conditionally inherit from BaseComponent for production build.
/// </remarks>
public class BaseComponent : BlazorStateDevToolsComponent
{
  public ApplicationState ApplicationState => GetState<ApplicationState>();
  internal CounterZeroState CounterZeroState => GetState<CounterZeroState>();
  internal EventStreamState EventStreamState => GetState<EventStreamState>();
  internal WeatherForecastsState WeatherForecastsState => GetState<WeatherForecastsState>();
  protected Task Send(IRequest aRequest) => Mediator.Send(aRequest);
}
