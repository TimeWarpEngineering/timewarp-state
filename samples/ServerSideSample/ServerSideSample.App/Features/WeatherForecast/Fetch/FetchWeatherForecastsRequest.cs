namespace ServerSideSample.App.Features.WeatherForecast
{
  using MediatR;

  public class FetchWeatherForecastsRequest : IRequest<WeatherForecastsState> { }
}