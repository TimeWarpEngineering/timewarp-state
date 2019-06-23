namespace TestApp.Client.Features.WeatherForecast
{
  using System.Collections.Generic;
  using System.Reflection;
  using BlazorState;
  using Microsoft.JSInterop;
  using TestApp.Shared.Features.WeatherForecast;
  using System.Text.Json.Serialization;
  using AnySerializer;

  internal partial class WeatherForecastsState : State<WeatherForecastsState>
  {
    public override WeatherForecastsState Hydrate(IDictionary<string, object> aKeyValuePairs)
    {
      string json = aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(WeatherForecasts))].ToString();
      List<WeatherForecastDto> newtonList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WeatherForecastDto>>(json);
      List<WeatherForecastDto> brokenList = JsonSerializer.Parse<List<WeatherForecastDto>>(json);

      // TODO remove NewtonSoft serializer when JsonSerializer actually works.

      var newWeatherForecastsState = new WeatherForecastsState
      {
        _WeatherForecasts = newtonList,
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