namespace BlazorState.Client.Services
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;

  public class WeatherForecastService
  {
    private static string[] s_Summaries = new[]
    {
      "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
    {
      var rng = new Random();
      return Task.FromResult(Enumerable.Range(1, 5).Select(aIndex => new WeatherForecast
      {
        Date = startDate.AddDays(aIndex),
        TemperatureC = rng.Next(-20, 55),
        Summary = s_Summaries[rng.Next(s_Summaries.Length)]
      }).ToArray());
    }
  }
}
