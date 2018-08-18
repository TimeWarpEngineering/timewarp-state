namespace BlazorState.Client.Features.WeatherForecast
{
  using System.Collections.Generic;
  using BlazorState.Shared;

  public class AjaxResponse
  {
    public List<WeatherForecast> WeatherForecasts { get; set; }
  }
}