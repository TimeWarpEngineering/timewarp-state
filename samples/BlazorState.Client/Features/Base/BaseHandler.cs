namespace BlazorState.Client.Features.Base
{
  using BlazorState.Client.Features.Application;
  using BlazorState.Client.Features.Counter;
  using BlazorState.Client.Features.WeatherForecast;
  using MediatR;

  /// <summary>
  /// Base Handler that makes it easy to access state
  /// </summary>
  /// <typeparam name="TRequest"></typeparam>
  /// <typeparam name="TState"></typeparam>
  public abstract class BaseHandler<TRequest, TState> : BlazorState.RequestHandler<TRequest, TState>
    where TRequest : IRequest<TState>
    where TState : IState
  {
    public BaseHandler(IStore aStore) : base(aStore) { }

    public CounterState CounterState => Store.GetState<CounterState>();
    private ApplicationState ApplicationState => Store.GetState<ApplicationState>();
    private WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();
  }
}