namespace Test.App.Contracts.Features.WeatherForecast;

using System;
using System.Collections.Generic;
using Test.App.Contracts.Features.Base;

public class GetWeatherForecastsResponse : BaseResponse
{
  /// <summary>
  /// a default constructor is required for deserialization
  /// </summary>
  public GetWeatherForecastsResponse() { }

  public GetWeatherForecastsResponse(Guid aCorrelationId)
  {
    WeatherForecasts = new List<WeatherForecastDto>();
    CorrelationId = aCorrelationId;
  }

  public required List<WeatherForecastDto> WeatherForecasts { get; set; }
}
