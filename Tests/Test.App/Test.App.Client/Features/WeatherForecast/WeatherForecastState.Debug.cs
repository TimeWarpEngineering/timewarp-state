namespace Test.App.Client.Features.WeatherForecast;

using System.Text.Json;

internal partial class WeatherForecastsState 
{
  private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
  
  public override WeatherForecastsState Hydrate(IDictionary<string, object> keyValuePairs)
  {
    string json = keyValuePairs[CamelCase.MemberNameToCamelCase(nameof(WeatherForecasts))].ToString() ?? throw new InvalidOperationException();

    var newWeatherForecastsState = new WeatherForecastsState
    {
      WeatherForecastList = 
        JsonSerializer.Deserialize<List<WeatherForecastDto>>(json, JsonSerializerOptions) ??
        throw new InvalidOperationException(),
      Guid = new Guid
      (
        keyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Guid))].ToString() ??
        throw new InvalidOperationException()
      )
    };

    return newWeatherForecastsState;
  }

  [UsedImplicitly]
  internal void Initialize(List<WeatherForecastDto> weatherForecastList)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    ArgumentNullException.ThrowIfNull(weatherForecastList);
  }
}
