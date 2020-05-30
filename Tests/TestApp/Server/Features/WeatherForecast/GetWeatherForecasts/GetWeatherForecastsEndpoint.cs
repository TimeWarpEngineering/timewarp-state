namespace TestApp.Server.Features.WeatherForecast
{
  using System.Threading.Tasks;
  using TestApp.Server.Features.Base;
  using TestApp.Api.Features.WeatherForecast;
  using Microsoft.AspNetCore.Mvc;

  public class GetWeatherForecastsEndpoint : BaseEndpoint<GetWeatherForecastsRequest, GetWeatherForecastsResponse>
  {
    [HttpGet(GetWeatherForecastsRequest.Route)]
    public async Task<IActionResult> Process(GetWeatherForecastsRequest aRequest) => await Send(aRequest);
  }
}