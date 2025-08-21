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
