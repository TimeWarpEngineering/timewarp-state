namespace Test.App.Client.Features.Base;

using Test.App.Client.Features.Application;
using Test.App.Client.Features.Counter;
using Test.App.Client.Features.EventStream;
using Test.App.Client.Features.WeatherForecast;

/// <summary>
/// Base Handler that makes it easy to access state
/// </summary>
/// <typeparam name="TAction">The Type of Action to be handled</typeparam>
internal abstract class BaseActionHandler<TAction> : ActionHandler<TAction>
  where TAction : IAction
{
  protected ApplicationState ApplicationState => Store.GetState<ApplicationState>();
  protected CounterZeroState CounterZeroState => Store.GetState<CounterZeroState>();
  protected EventStreamState EventStreamState => Store.GetState<EventStreamState>();
  protected WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();

  public BaseActionHandler(IStore aStore) : base(aStore) { }
}
