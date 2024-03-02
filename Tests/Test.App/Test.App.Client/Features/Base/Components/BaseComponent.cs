namespace Test.App.Client.Features.Base.Components;

using CloneTest;

/// <summary>
/// Makes access to the State a little easier and by inheriting from
/// BlazorStateDevToolsComponent it allows for ReduxDevTools operation.
/// </summary>
/// <remarks>
/// In production one would NOT be required to use these base components
/// But would be required to properly implement the required interfaces.
/// one could conditionally inherit from BaseComponent for production build.
/// </remarks>
public abstract class BaseComponent : BlazorStateDevToolsComponent
{
  protected ApplicationState ApplicationState => GetState<ApplicationState>();
  internal BlueState BlueState => GetState<BlueState>();
  internal CounterState CounterState => GetState<CounterState>();
  internal CloneableState CloneableState => GetState<CloneableState>();
  internal EventStreamState EventStreamState => GetState<EventStreamState>();
  internal PurpleState PurpleState => GetState<PurpleState>();
  internal ProcessingState ProcessingState => GetState<ProcessingState>();
  internal RouteState RouteState => GetState<RouteState>();
  internal ThemeState ThemeState => GetState<ThemeState>();
  internal WeatherForecastsState WeatherForecastsState => GetState<WeatherForecastsState>();
  
  // ReSharper disable once UnusedMember.Global - Can be used by inheriting classes not in this library.
  protected Task Send(IRequest aRequest) => Mediator.Send(aRequest);
}
