#region Purpose
// Server-scoped mediator request for the weather-forecasts API endpoint.
#endregion

#region Design
// Contracts.GetWeatherForecasts.Query is an HTTP DTO in a shared assembly with no TimeWarp.State
// reference and no MediatorScope, so it belongs to the unscoped default pipeline. Sending that
// Query through ISender<ServerPipeline> is TWM004. This server-local request carries
// MediatorScope(ServerPipeline), implements IRequest of the contracts Response, and is what
// MapGet Sends. The contracts Query stays the HTTP route/DTO the client GETs.
#endregion

namespace Test.App.Server.Features.WeatherForecast;

using Contracts.Features.WeatherForecast;
using TimeWarp.Mediator;
using TimeWarp.State;

[MediatorScope(typeof(ServerPipeline))]
public sealed class GetWeatherForecastsRequest : IRequest<GetWeatherForecasts.Response>
{
  public int Days { get; init; }
}
