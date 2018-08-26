namespace BlazorState.Shared.Features.WeatherForecast
{
  using BlazorState.Shared.Features.Base;
  using MediatR;

  public class GetWeatherForecastsRequest : BaseRequest, IRequest<GetWeatherForecastsResponse>
  {
    public const string Route = "api/weatherForecast";
  }
}