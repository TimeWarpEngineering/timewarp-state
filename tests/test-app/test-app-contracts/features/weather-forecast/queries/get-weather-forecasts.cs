#region Purpose
// Weather forecast query, response and DTO shared by the client and the server API.
#endregion

#region Design
// Query.GetRoute appends Days as a query string on "api/weather". Response is a List of WeatherForecastDto, and
// TemperatureF is computed from TemperatureC.
#endregion

namespace Test.App.Contracts.Features.WeatherForecast;

public static class GetWeatherForecasts
{
  public sealed class Query : IRequest<Response>
  {
    public int Days { get; init; }

    public const string RouteTemplate = "api/weather";
    public string GetRoute() => FormattableString.Invariant($"{RouteTemplate}?{nameof(Days)}={Days}");
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
