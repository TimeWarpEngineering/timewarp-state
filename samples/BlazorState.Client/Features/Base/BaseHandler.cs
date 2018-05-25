using BlazorState.Client.State;
using MediatR;

namespace BlazorState.Client.Features.Base
{
  /// <summary>
  /// Similare
  /// </summary>
  /// <typeparam name="TRequest"></typeparam>
  /// <typeparam name="TState"></typeparam>
  public abstract class BaseHandler<TRequest, TState> : RequestHandler<TRequest, TState>
    where TRequest : IRequest<TState>
    where TState : IState
  {
    public BaseHandler(IStore aStore) : base(aStore) { }

    public CounterState CounterState => Store.GetState<CounterState>();
    private ApplicationState ApplicationState => Store.GetState<ApplicationState>();
    private WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();
  }
}