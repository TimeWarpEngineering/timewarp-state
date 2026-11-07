#region Purpose
// Handles the server-scoped weather-forecasts request for the API endpoint.
#endregion

#region Design
// Bound to ServerPipeline so it never runs ClientPipeline store behaviors. Consumes the
// server-local GetWeatherForecastsRequest (not the contracts Query HTTP DTO) and returns the
// shared contracts Response. Assembly MediatorScope is reinforced on the type for TWM004 clarity.
// Calls and results are logged through ILogger because Console is banned (RS0030).
#endregion

namespace Test.App.Server.Features.WeatherForecast;

[MediatorScope(typeof(ServerPipeline))]
public sealed class GetWeatherForecastsHandler : IRequestHandler<GetWeatherForecastsRequest, GetWeatherForecasts.Response>
{
  private readonly ILogger<GetWeatherForecastsHandler> Logger;

  public GetWeatherForecastsHandler(ILogger<GetWeatherForecastsHandler> logger)
  {
    Logger = logger;
  }

  public ValueTask<GetWeatherForecasts.Response> Handle(GetWeatherForecastsRequest request, CancellationToken cancellationToken)
  {
    Logger.LogInformation("Weather API endpoint called for {Days} days", request.Days);

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

    Logger.LogInformation("Generated {Count} weather forecasts", forecasts.Length);

    GetWeatherForecasts.Response response = new();
    response.AddRange(forecasts);
    return new ValueTask<GetWeatherForecasts.Response>(response);
  }
}
