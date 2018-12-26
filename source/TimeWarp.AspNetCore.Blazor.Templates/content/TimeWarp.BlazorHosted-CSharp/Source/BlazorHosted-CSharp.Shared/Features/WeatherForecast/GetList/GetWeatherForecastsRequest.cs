namespace BlazorHosted_CSharp.Shared.Features.WeatherForecast
{
  using BlazorHosted_CSharp.Shared.Features.Base;
  using MediatR;

  public class GetWeatherForecastsRequest : BaseRequest, IRequest<GetWeatherForecastsResponse>
  {
    public const string Route = "api/weatherForecast";
  }
}