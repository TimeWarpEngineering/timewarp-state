#region Purpose
// WeatherForecastsState: the forecasts loaded by FetchWeatherForecasts, shown on the WASM weather page.
#endregion

#region Design
// The response list is a private field exposed as a read-only list; Initialize clears it.
#endregion

namespace Test.App.Client.Features.WeatherForecast;

using static Contracts.Features.WeatherForecast.GetWeatherForecasts;

public sealed partial class WeatherForecastsState: State<WeatherForecastsState>
{
  private Response? WeatherForecastList;

  public IReadOnlyList<WeatherForecastDto>? WeatherForecasts => WeatherForecastList?.AsReadOnly();

  ///<inheritdoc/>
  public override void Initialize()
  {
    WeatherForecastList = null;
  }
}
