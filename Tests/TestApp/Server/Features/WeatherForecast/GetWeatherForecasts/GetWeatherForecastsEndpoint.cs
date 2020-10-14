namespace TestApp.Server.Features.WeatherForecast
{
  using Microsoft.AspNetCore.Mvc;
  using System.Threading.Tasks;
  using TestApp.Api.Features.WeatherForecast;
  using TestApp.Server.Features.Base;

  public class GetWeatherForecastsEndpoint : BaseEndpoint<GetWeatherForecastsRequest, GetWeatherForecastsResponse>
  {
    [HttpGet(GetWeatherForecastsRequest.Route)]
    public async Task<IActionResult> Process(GetWeatherForecastsRequest aRequest) => await Send(aRequest);
  }
}