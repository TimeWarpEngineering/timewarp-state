namespace Test.App.Client.Features.WeatherForecast;

internal partial class WeatherForecastsState
{
  private List<WeatherForecastDto> WeatherForecastList = [];

  public IReadOnlyList<WeatherForecastDto> WeatherForecasts => WeatherForecastList.AsReadOnly();

  ///<inheritdoc/>
  public override void Initialize() => ArgumentNullException.ThrowIfNull(WeatherForecastList);
}
