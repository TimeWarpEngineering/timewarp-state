namespace BlazorState.Client.State
{
  using System.Collections.Generic;
  using System.Linq;
  using BlazorState.Shared;
  using BlazorState.State;

  public class WeatherForecastsState : State<WeatherForecastsState>
  {
    public WeatherForecastsState()
    {
      WeatherForecasts = new List<WeatherForecast>();
    }

    protected WeatherForecastsState(WeatherForecastsState aState) : this()
    {
      foreach (WeatherForecast forecast in aState.WeatherForecasts)
      {
        WeatherForecasts.Add(forecast.Clone() as WeatherForecast);
      }
    }

    public List<WeatherForecast> WeatherForecasts { get; set; }

    public override object Clone() => new WeatherForecastsState(this);

    //public override string ToString()
    //{
    //  string weatherForecasts = string.Join("\n", WeatherForecasts.Select(f => f.ToString()));

    //  return $"Forecasts:\n\n{weatherForecasts}";
    //}

    protected override void Initialize() { }
  }
}