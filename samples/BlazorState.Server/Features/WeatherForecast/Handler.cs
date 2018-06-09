namespace BlazorState.Server.Features.WeatherForecast
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState.Shared;
  using MediatR;

  public class Handler : IRequestHandler<Request, Response>
  {
    private static string[] Summaries = new[]
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

    public async Task<Response> Handle(Request aRequest, CancellationToken cancellationToken)
    {
      var response = new Response(aRequest.Id);
      var random = new Random();
      var weatherForecasts = new List<WeatherForecast>();
      Enumerable.Range(1, 5).ToList().ForEach(index => response.WeatherForecasts.Add(
        new WeatherForecast
        {
          Date = DateTime.Now.AddDays(index),
          TemperatureC = random.Next(-20, 55),
          Summary = Summaries[random.Next(Summaries.Length)]
        }));

      return await Task.Run(() => response);
    }
  }
}