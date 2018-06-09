namespace BlazorState.Server.Features.WeatherForecast
{
  using BlazorState.Server.Features.Base;
  using MediatR;

  public class Request : BaseRequest, IRequest<Response> { }
}