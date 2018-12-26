namespace BlazorHosted_CSharp.Server.Features.WeatherForecast
{
  using System.Threading.Tasks;
  using BlazorHosted_CSharp.Server.Features.Base;
  using BlazorHosted_CSharp.Shared.Features.WeatherForecast;
  using MediatR;
  using Microsoft.AspNetCore.Mvc;

  [Route(GetWeatherForecastsRequest.Route)]
  public class GetWeatherForecastsController : BaseController<GetWeatherForecastsRequest, GetWeatherForecastsResponse>
  {
    public GetWeatherForecastsController(IMediator aMediator) : base(aMediator) { }

    public async Task<IActionResult> Get(GetWeatherForecastsRequest aRequest) => await Send(aRequest);
  }
}