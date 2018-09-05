namespace BlazorHosted_CSharp.Client.Components
{
  using BlazorState.Behaviors.ReduxDevTools;
  using BlazorHosted_CSharp.Client.Features.Application;
  using BlazorHosted_CSharp.Client.Features.Counter;
  using BlazorHosted_CSharp.Client.Features.WeatherForecast;

  /// <summary>
  /// Makes access to the State a little easier and by inheriting from
  /// BlazorStateDevToolsComponent it allows for ReduxDevTools operation.
  /// </summary>
  /// <remarks>
  /// In production one would NOT be required to use these base components
  /// But would be required to properly implement the required interfaces.
  /// one could conditionally inherit from BlazorComponent for production build.
  /// </remarks>
  public class BaseComponent : BlazorStateDevToolsComponent
  {
    public ApplicationState ApplicationState => Store.GetState<ApplicationState>();
    public CounterState CounterState => Store.GetState<CounterState>();
    public WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();
  }
}