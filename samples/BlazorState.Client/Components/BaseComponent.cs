namespace BlazorState.Client.Components
{
  using BlazorState.Behaviors.ReduxDevTools;
  using BlazorState.Client.State;

  /// <summary>
  /// Makes access the State a little easier and by inheriting from
  /// BlazorStateDevToolsComponent it allows for ReduxDevTools operation.
  /// </summary>
  /// <remarks>
  /// In production one would NOT be required to use these base components
  /// But would be required to properly implement the required interfaces.
  /// </remarks>
  public class BaseComponent : BlazorStateDevToolsComponent
  {
    public ApplicationState ApplicationState => Store.GetState<ApplicationState>();
    public CounterState CounterState => Store.GetState<CounterState>();
    public WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();
  }
}