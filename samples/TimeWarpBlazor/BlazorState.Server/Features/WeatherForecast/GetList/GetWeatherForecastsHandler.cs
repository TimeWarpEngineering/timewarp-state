namespace BlazorState.Server.Features.WeatherForecast
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState.Shared.Features.WeatherForecast;
  using MediatR;

  public class GetWeatherForecastsHandler : IRequestHandler<GetWeatherForecastsRequest, GetWeatherForecastsResponse>
  {
    private string[] Summaries = new[]
    {
      "Freezing",
      "Bracing",
      "Chilly",
      "Cool",
      "Mild",
      "Warm",
      "Balmy",
      "Hot",
      "Sweltering",
      "Scorching"
    };

    public async Task<GetWeatherForecastsResponse> Handle(
      GetWeatherForecastsRequest aRequest,
      CancellationToken aCancellationToken)
    {
      var response = new GetWeatherForecastsResponse(aRequest.Id);
      var random = new Random();
      var weatherForecasts = new List<WeatherForecastDto>();
      Enumerable.Range(1, 5).ToList().ForEach(aIndex => response.WeatherForecasts.Add(
        new WeatherForecastDto
        {
          Date = DateTime.Now.AddDays(aIndex),
          TemperatureC = random.Next(-20, 55),
          Summary = Summaries[random.Next(Summaries.Length)]
        }));

      var AssemblyVersion = @Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion;
      return await Task.Run(() => response);
    }
  }
}