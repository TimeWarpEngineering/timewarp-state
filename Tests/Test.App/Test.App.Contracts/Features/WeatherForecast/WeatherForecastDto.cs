namespace Test.App.Contracts.Features.WeatherForecast;

using System;

/// <summary>
/// The object that is passed back and forth from the Test.App.Server to the client.
/// </summary>
/// <remarks>TODO: This should be an immutable class 
/// but serialization doesn't work with no setter or private setter yet</remarks>
public class WeatherForecastDto
{
  public WeatherForecastDto() { }

  public WeatherForecastDto(DateTime date, string summary, int temperatureC)
  {
    Date = date;
    Summary = summary;
    TemperatureC = temperatureC;
  }

  public DateTime Date { get; }
  public required string Summary { get; init; }
  public int TemperatureC { get; set; }
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
