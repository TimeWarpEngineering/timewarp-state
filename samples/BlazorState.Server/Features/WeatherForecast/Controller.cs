namespace BlazorState.Server.Features.WeatherForecast
{
  using System.Threading.Tasks;
  using BlazorState.Server.Features.Base;
  using MediatR;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  [Route(Route, Name = RouteName)]
  public class Controller : BaseController<Request, Response>
  {
    public const string Route = "api/weatherForecast";
    public const string RouteName = "Features.WeatherForecast";

    public Controller(
      ILogger<BaseController<Request, Response>> aLogger,
      IMediator aMediator)
      : base(aLogger, aMediator)
    { }

    public async Task<IActionResult> Get(Request aRequest) => await Send(aRequest);
  }
}