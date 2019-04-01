namespace TestApp.Client.Components
{
  using BlazorState.Behaviors.ReduxDevTools;
  using TestApp.Client.Features.Application;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Features.WeatherForecast;

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
    public CounterState CounterState => GetState<CounterState>();
    public WeatherForecastsState WeatherForecastsState => GetState<WeatherForecastsState>();
  }
}