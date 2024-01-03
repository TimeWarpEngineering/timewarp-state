namespace TestApp.Client.Features.Base;

using BlazorState;
using TestApp.Client.Features.Application;
using TestApp.Client.Features.Counter;
using TestApp.Client.Features.EventStream;
using TestApp.Client.Features.WeatherForecast;

/// <summary>
/// Base Handler that makes it easy to access state
/// </summary>
/// <typeparam name="TAction">The Type of Action to be handled</typeparam>
internal abstract class BaseActionHandler<TAction> : ActionHandler<TAction>
  where TAction : IAction
{
  protected ApplicationState ApplicationState => Store.GetStateAsync<ApplicationState>();
  protected CounterState CounterState => Store.GetStateAsync<CounterState>();
  protected EventStreamState EventStreamState => Store.GetStateAsync<EventStreamState>();
  protected WeatherForecastsState WeatherForecastsState => Store.GetStateAsync<WeatherForecastsState>();

  public BaseActionHandler(IStore aStore) : base(aStore) { }
}
