namespace TestApp.Client.Features.WeatherForecast
{
  using BlazorState;
  using Microsoft.JSInterop;
  using System.Collections.Generic;
  using System.Reflection;
  using System.Text.Json;
  using TestApp.Api.Features.WeatherForecast;

  internal partial class WeatherForecastsState : State<WeatherForecastsState>
  {
    public override WeatherForecastsState Hydrate(IDictionary<string, object> aKeyValuePairs)
    {
      string json = aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(WeatherForecasts))].ToString();

      var newWeatherForecastsState = new WeatherForecastsState()
      {
        _WeatherForecasts = JsonSerializer.Deserialize<List<WeatherForecastDto>>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
        Guid = new System.Guid(aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Guid))].ToString()),
      };

      return newWeatherForecastsState;
    }

    internal void Initialize(List<WeatherForecastDto> aWeatherForecastList)
    {
      ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
      _WeatherForecasts = aWeatherForecastList ??
        throw new System.ArgumentNullException(nameof(aWeatherForecastList));
    }
  }
}