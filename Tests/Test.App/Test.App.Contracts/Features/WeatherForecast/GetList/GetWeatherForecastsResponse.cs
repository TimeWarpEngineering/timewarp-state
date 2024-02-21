namespace Test.App.Contracts.Features.WeatherForecast;

public class GetWeatherForecastsResponse
{
  public required List<WeatherForecastDto> WeatherForecasts { get; init; } = [];
}
