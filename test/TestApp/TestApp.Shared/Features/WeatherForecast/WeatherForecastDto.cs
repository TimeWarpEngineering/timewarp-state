namespace TestApp.Shared.Features.WeatherForecast
{
  using System;

  public class WeatherForecastDto
  {
    public WeatherForecastDto() { }

    public WeatherForecastDto(DateTime aDate, string aSummary, int aTemperatureC)
    {
      Date = aDate;
      Summary = aSummary;
      TemperatureC = aTemperatureC;
    }

    public DateTime Date { get; }
    public string Summary { get; }
    public int TemperatureC { get; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
  }
}