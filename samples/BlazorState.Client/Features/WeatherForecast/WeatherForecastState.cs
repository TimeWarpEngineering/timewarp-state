namespace BlazorState.Client.Features.WeatherForecast
{
  using System.Collections.Generic;
  using System.Reflection;
  using BlazorState;
  using BlazorState.Shared;

  public partial class WeatherForecastsState : State<WeatherForecastsState>
  {
    private List<WeatherForecast> _WeatherForecasts;

    public WeatherForecastsState()
    {
      _WeatherForecasts = new List<WeatherForecast>();
    }

    protected WeatherForecastsState(WeatherForecastsState aState) : this()
    {
      foreach (WeatherForecast forecast in aState.WeatherForecasts)
      {
        _WeatherForecasts.Add(forecast.Clone() as WeatherForecast);
      }
    }

    public IReadOnlyList<WeatherForecast> WeatherForecasts => _WeatherForecasts.AsReadOnly();

    public override object Clone() => new WeatherForecastsState(this);

    protected override void Initialize() { }

    private void Initialize(List<WeatherForecast> aWeatherForecastList)
    {
      ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
      _WeatherForecasts = aWeatherForecastList ??
        throw new System.ArgumentNullException(nameof(aWeatherForecastList));
    }
  }
}