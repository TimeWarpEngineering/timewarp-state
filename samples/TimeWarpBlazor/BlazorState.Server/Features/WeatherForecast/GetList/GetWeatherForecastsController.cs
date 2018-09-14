namespace BlazorState.Server.Features.WeatherForecast
{
  using System.Threading.Tasks;
  using BlazorState.Server.Features.Base;
  using BlazorState.Shared.Features.WeatherForecast;
  using MediatR;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  [Route(GetWeatherForecastsRequest.Route)]
  public class Controller : BaseController<GetWeatherForecastsRequest, GetWeatherForecastsResponse>
  {
    public Controller(
      ILogger<BaseController<GetWeatherForecastsRequest, GetWeatherForecastsResponse>> aLogger,
      IMediator aMediator)
      : base(aLogger, aMediator)
    { }

    public async Task<IActionResult> Get(GetWeatherForecastsRequest aRequest) => await Send(aRequest);
  }
}