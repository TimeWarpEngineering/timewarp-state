#if DEBUG
namespace ServerSideSample.App.Features.WeatherForecast
{
  using System.Collections.Generic;
  using BlazorState;
  using ServerSideSample.Shared.Features.WeatherForecast;
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
#endif