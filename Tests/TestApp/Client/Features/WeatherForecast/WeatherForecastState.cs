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


    /// <summary>
    /// 
    /// </summary>
    /// <remarks>used to test that constructor is complete before Initialize is called</remarks>
    public override void Initialize() 
    {
      if (_WeatherForecasts is null)
      {
        throw new System.ArgumentNullException(nameof(_WeatherForecasts));
      }

    }
  }
}