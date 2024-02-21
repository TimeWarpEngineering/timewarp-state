namespace Test.App.Client.Features.WeatherForecast;

internal partial class WeatherForecastsState: State<WeatherForecastsState>
{
  private List<WeatherForecastDto> WeatherForecastList = [];

  public IReadOnlyList<WeatherForecastDto> WeatherForecasts => WeatherForecastList.AsReadOnly();

  ///<inheritdoc/>
  public override void Initialize() => ArgumentNullException.ThrowIfNull(WeatherForecastList);
}
