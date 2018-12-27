namespace BlazorHosted_CSharp.Client.Features.WeatherForecast
{
  using MediatR;

  public class FetchWeatherForecastsRequest : IRequest<WeatherForecastsState> { }
}