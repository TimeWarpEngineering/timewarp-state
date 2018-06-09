namespace BlazorState.Server.Features.WeatherForecast
{
  using System;
  using System.Collections.Generic;
  using BlazorState.Server.Features.Base;
  using BlazorState.Shared;

  public class Response : BaseResponse
  {
    public Response(Guid aRequestId)
    {
      WeatherForecasts = new List<WeatherForecast>();
      RequestId = aRequestId;
    }

    public List<WeatherForecast> WeatherForecasts { get; set; }
  }
}