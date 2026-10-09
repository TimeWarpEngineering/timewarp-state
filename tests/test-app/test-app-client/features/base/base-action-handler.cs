#region Purpose
// Base handler for test-app actions with shortcut properties for the app's common states.
#endregion

#region Design
// Derives from StateActionHandler<TAction> and only adds Store.GetState<T>() wrappers (Application, Counter,
// EventStream, WeatherForecasts, CacheableWeather); it adds no behavior.
#endregion

// ReSharper disable UnusedMember.Global
namespace Test.App.Client.Features.Base;

/// <summary>
/// Base Handler that makes it easy to access state
/// </summary>
/// <typeparam name="TAction">The Type of Action to be handled</typeparam>
internal abstract class BaseActionHandler<TAction>
(
  IStore store
) : StateActionHandler<TAction>(store)
  where TAction : IAction
{
  protected ApplicationState ApplicationState => Store.GetState<ApplicationState>();
  protected CounterState CounterState => Store.GetState<CounterState>();
  protected EventStreamState EventStreamState => Store.GetState<EventStreamState>();
  protected WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();
  protected CacheableWeatherState CacheableWeatherState => Store.GetState<CacheableWeatherState>();
}
