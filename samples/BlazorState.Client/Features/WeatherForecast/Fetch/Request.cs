namespace BlazorState.Client.Features.WeatherForecast.Fetch
{
  using BlazorState.Client.State;
  using MediatR;

  public class Request : IRequest<WeatherForecastsState> { }
}