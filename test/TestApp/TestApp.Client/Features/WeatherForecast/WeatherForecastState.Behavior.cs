namespace TestApp.Client.Features.WeatherForecast
{
  using BlazorState;
  using TestApp.Shared.Features.WeatherForecast;

  public partial class WeatherForecastsState : State<WeatherForecastsState>
  {
    private WeatherForecastsState(WeatherForecastsState aState) : this()
    {
      foreach (WeatherForecastDto weatherForecastDto in aState.WeatherForecasts)
      {
        _WeatherForecasts.Add(weatherForecastDto.Clone() as WeatherForecastDto);
      }
    }

    public override object Clone() => new WeatherForecastsState(this);

    protected override void Initialize() { }
  }
}