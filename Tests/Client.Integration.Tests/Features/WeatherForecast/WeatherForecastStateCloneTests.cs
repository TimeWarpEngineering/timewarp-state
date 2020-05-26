namespace TestApp.Client.Integration.Tests.Features.WeatherForecast_Tests
{
  using System;
  using Shouldly;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using System.Collections.Generic;
  using TestApp.Client.Features.WeatherForecast;
  using TestApp.Api.Features.WeatherForecast;
  using AnyClone;

  internal class WeatherForecastStateCloneTests: BaseTest
  {
    public WeatherForecastStateCloneTests(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost)
    {
      WeatherForecastsState = Store.GetState<WeatherForecastsState>();
    }

    private WeatherForecastsState WeatherForecastsState { get; set; }

    public void ShouldClone()
    {
      //Arrange
      var weatherForecasts = new List<WeatherForecastDto> {
        new WeatherForecastDto
        (
          aDate: DateTime.MinValue,
          aSummary: "Summary 1",
          aTemperatureC: 24
        ),
        new WeatherForecastDto
        (
          aDate: DateTime.MinValue,
          aSummary: "Summary 1",
          aTemperatureC: 24
        ),
      };
      WeatherForecastsState.Initialize(weatherForecasts);

      //Act
      var clone = WeatherForecastsState.Clone() as WeatherForecastsState;

      //Assert
      WeatherForecastsState.ShouldNotBeSameAs(clone);
      WeatherForecastsState.WeatherForecasts.Count.ShouldBe(clone.WeatherForecasts.Count);
      WeatherForecastsState.Guid.ShouldNotBe(clone.Guid);
      WeatherForecastsState.WeatherForecasts[0].TemperatureC.ShouldBe(clone.WeatherForecasts[0].TemperatureC);
      WeatherForecastsState.WeatherForecasts[0].ShouldNotBe(clone.WeatherForecasts[0]);
    }

  }
}
