namespace TestApp.Client.Features.Base
{
  using TestApp.Client.Features.Application;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Features.WeatherForecast;
  using TestApp.Client.Features.EventStream;
  using BlazorState;
  using MediatR;

  /// <summary>
  /// Base Handler that makes it easy to access state
  /// </summary>
  /// <typeparam name="TRequest"></typeparam>
  /// <typeparam name="TState"></typeparam>
  internal abstract class BaseHandler<TRequest, TState> : BlazorState.RequestHandler<TRequest, TState>
    where TRequest : IRequest<TState>
    where TState : IState
  {
    public BaseHandler(IStore aStore) : base(aStore) { }

    protected ApplicationState ApplicationState => Store.GetState<ApplicationState>();
    protected WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();
    protected CounterState CounterState => Store.GetState<CounterState>();
    protected EventStreamState EventStreamState => Store.GetState<EventStreamState>();
  }
}