namespace TestApp.Client.Features.Base
{
  using BlazorState;
  using TestApp.Client.Features.Application;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Features.EventStream;
  using TestApp.Client.Features.WeatherForecast;

  /// <summary>
  /// Base Handler that makes it easy to access state
  /// </summary>
  /// <typeparam name="TAction">The Type of Action to be handled</typeparam>
  internal abstract class BaseHandler<TAction> : ActionHandler<TAction>
    where TAction : IAction
  {
    protected ApplicationState ApplicationState => Store.GetState<ApplicationState>();
    protected CounterState CounterState => Store.GetState<CounterState>();
    protected EventStreamState EventStreamState => Store.GetState<EventStreamState>();
    protected WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();

    public BaseHandler(IStore aStore) : base(aStore) { }
  }
}