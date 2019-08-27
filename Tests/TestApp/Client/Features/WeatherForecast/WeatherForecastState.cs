namespace TestApp.Client.Features.WeatherForecast
{
  using System.Collections.Generic;
  using BlazorState;
  using TestApp.Api.Features.WeatherForecast;

  internal partial class WeatherForecastsState : State<WeatherForecastsState>
  {
    private List<WeatherForecastDto> _WeatherForecasts;

    public IReadOnlyList<WeatherForecastDto> WeatherForecasts => _WeatherForecasts.AsReadOnly();

    public WeatherForecastsState()
    {
      _WeatherForecasts = new List<WeatherForecastDto>();
    }

    protected override void Initialize() { }
  }
}