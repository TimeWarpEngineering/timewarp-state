namespace BlazorState.Shared
{
  using System;

  public class WeatherForecast : ICloneable
  {
    public WeatherForecast() { }

    protected WeatherForecast(WeatherForecast aWeatherForecast)
    {
      Date = new DateTime(aWeatherForecast.Date.Ticks);
      TemperatureC = aWeatherForecast.TemperatureC;
      Summary = aWeatherForecast.Summary;
    }

    public DateTime Date { get; set; }
    public string Summary { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public object Clone()
    {
      return new WeatherForecast(this);
    }
  }
}