namespace TestApp.Client.Features.WeatherForecast
{
  using System.Collections.Generic;
  using TestApp.Shared.Features.WeatherForecast;
  using BlazorState;
  using Microsoft.JSInterop;

  public partial class WeatherForecastsState : State<WeatherForecastsState>
  {
    public override WeatherForecastsState Hydrate(IDictionary<string, object> aKeyValuePairs)
    {
      var newWeatherForecastsState = new WeatherForecastsState
      {
        _WeatherForecasts = Json.Deserialize<List<WeatherForecastDto>>(
          aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(WeatherForecasts))].ToString()),
        Guid = new System.Guid((string)aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Guid))]),
      };

      return newWeatherForecastsState;
    }
  }
}
