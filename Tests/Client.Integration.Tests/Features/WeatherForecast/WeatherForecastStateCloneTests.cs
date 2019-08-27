namespace TestApp.Client.Integration.Tests.Features.WeatherForecast
{
  using System;
  using BlazorState;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using System.Collections.Generic;
  using TestApp.Client.Features.WeatherForecast;
  using TestApp.Api.Features.WeatherForecast;
  using AnyClone;

  internal class WeatherForecastStateCloneTests
  {
    public WeatherForecastStateCloneTests(TestFixture aTestFixture)
    {
      IServiceProvider serviceProvider = aTestFixture.ServiceProvider;
      IStore store = serviceProvider.GetService<IStore>();
      WeatherForecastsState = store.GetState<WeatherForecastsState>();
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
