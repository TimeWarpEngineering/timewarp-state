namespace ServerSideSample.App.Components
{
  using BlazorState.Behaviors.ReduxDevTools;
  using ServerSideSample.App.Features.Application;
  using ServerSideSample.App.Features.Counter;
  using ServerSideSample.App.Features.WeatherForecast;

  public class BaseComponent : BlazorStateDevToolsComponent
  {
    public ApplicationState ApplicationState => Store.GetState<ApplicationState>();
    public CounterState CounterState => Store.GetState<CounterState>();
    public WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();
  }
}