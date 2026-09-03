#region Purpose
// Handles the server-scoped weather-forecasts request for the API endpoint.
#endregion

#region Design
// Bound to ServerPipeline so it never runs ClientPipeline store behaviors. Consumes the
// server-local GetWeatherForecastsRequest (not the contracts Query HTTP DTO) and returns the
// shared contracts Response. Assembly MediatorScope is reinforced on the type for TWM004 clarity.
#endregion

namespace Test.App.Server.Features.WeatherForecast;

using Contracts.Features.WeatherForecast;
using TimeWarp.Mediator;
using TimeWarp.State;

[MediatorScope(typeof(ServerPipeline))]
public sealed class GetWeatherForecastsHandler : IRequestHandler<GetWeatherForecastsRequest, GetWeatherForecasts.Response>
{
  public Task<GetWeatherForecasts.Response> Handle(GetWeatherForecastsRequest request, CancellationToken cancellationToken)
  {
    Console.WriteLine("Weather API endpoint called at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

    DateOnly startDate = DateOnly.FromDateTime(DateTime.Now);
    string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];
    GetWeatherForecasts.WeatherForecastDto[] forecasts =
      Enumerable.Range(1, request.Days)
        .Select
        (
          index =>
            new GetWeatherForecasts.WeatherForecastDto
            (
              startDate.AddDays(index),
              summaries[Random.Shared.Next(summaries.Length)],
              Random.Shared.Next(-20, 55)
            )
        ).ToArray();

    Console.WriteLine($"Generated {forecasts.Length} weather forecasts");

    GetWeatherForecasts.Response response = new();
    response.AddRange(forecasts);
    return Task.FromResult(response);
  }
}
