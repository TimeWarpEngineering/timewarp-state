namespace Test.App.Contracts.Features.WeatherForecast;

public class GetWeatherForecastsRequest : IRequest<GetWeatherForecastsResponse>
{
  public const string Route = "api/weatherForecast";
  /// <summary>
  /// The Number of days of forecasts to get
  /// </summary>
  public int Days { get; init; }

  [JsonIgnore]
  public string RouteFactory => $"{Route}?{nameof(Days)}={Days}";
}
