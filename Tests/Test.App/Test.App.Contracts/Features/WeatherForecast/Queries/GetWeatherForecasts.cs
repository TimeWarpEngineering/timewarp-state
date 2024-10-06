namespace Test.App.Contracts.Features.WeatherForecast;

public static class GetWeatherForecasts
{
  public sealed class Query : IRequest<Response>
  {
    public int Days { get; init; }

    public const string RouteTemplate = "api/weather";
    public string GetRoute() => FormattableString.Invariant($"{RouteTemplate}");
  }

  public sealed class Response : List<WeatherForecastDto>;

  public sealed class WeatherForecastDto
  {
    public DateOnly Date { get; }

    public string Summary { get; }

    public int TemperatureC { get; }


    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public WeatherForecastDto(DateOnly date, string summary, int temperatureC)
    {
      Date = date;
      Summary = summary;
      TemperatureC = temperatureC;
    }
  }
}
