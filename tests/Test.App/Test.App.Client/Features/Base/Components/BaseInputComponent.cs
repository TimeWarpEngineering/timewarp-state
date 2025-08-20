// ReSharper disable UnusedMember.Global
namespace Test.App.Client.Features.Base.Components;

/// <summary>
/// Makes access to the State a little easier and by inheriting from
/// TimeWarpStateDevToolsInputComponent it allows for ReduxDevTools operation.
/// </summary>
/// <remarks>
/// In production one would NOT be required to use these base components
/// But would be required to properly implement the required interfaces.
/// one could conditionally inherit from BaseComponent for production build.
/// </remarks>
public abstract class BaseInputComponent<T> : TimeWarpStateInputComponent<T>
{
  internal ApplicationState ApplicationState => GetState<ApplicationState>();
  internal CounterState CounterState => GetState<CounterState>();
  internal EventStreamState EventStreamState => GetState<EventStreamState>();
  internal WeatherForecastsState WeatherForecastsState => GetState<WeatherForecastsState>();
  internal ThemeState ThemeState => GetState<ThemeState>();
}
