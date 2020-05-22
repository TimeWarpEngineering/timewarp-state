namespace BlazorState.Features.JavaScriptInterop.JsonRequestHandler_Tests
{
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using System;
  using System.Threading.Tasks;
  using TestApp.Server.Integration.Tests.Infrastructure;
  using TestApp.Api.Features.WeatherForecast;
  using FluentAssertions;

  internal class Handle_Returns
  {
    private readonly IMediator Mediator;

    public Handle_Returns(TestFixture aTestFixture)
    {
      IServiceProvider serviceProvider = aTestFixture.ServiceProvider;
      Mediator = serviceProvider.GetService<IMediator>();
    }

    public async Task _10WeatherForecasts_Given_10DaysRequested()
    {
      // Arrange
      var getWeatherForecastsRequest = new GetWeatherForecastsRequest { Days = 10 };

      //Act
      GetWeatherForecastsResponse getWeatherForecastsResponse =
        await Mediator.Send(getWeatherForecastsRequest);

      //Assert
      getWeatherForecastsResponse.WeatherForecasts.Count.ShouldBe(10);
      getWeatherForecastsResponse.WeatherForecasts.Count.Should().Be(10);
    }
  }
}