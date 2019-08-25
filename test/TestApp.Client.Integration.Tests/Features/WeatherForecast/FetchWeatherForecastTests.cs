namespace TestApp.Client.Integration.Tests.Features.WeatherForecast
{
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using System;
  using System.Threading.Tasks;
  using TestApp.Client.Features.WeatherForecast;
  using TestApp.Client.Integration.Tests.Infrastructure;

  internal class FetchWeatherForecastTests
  {
    private IMediator Mediator { get; }

    private IServiceProvider ServiceProvider { get; }

    private IStore Store { get; }

    private WeatherForecastsState WeatherForecastsState { get; set; }

    public FetchWeatherForecastTests(TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
      Mediator = ServiceProvider.GetService<IMediator>();
      Store = ServiceProvider.GetService<IStore>();
      WeatherForecastsState = Store.GetState<WeatherForecastsState>();
    }

    public async Task Should_Fetch_WeatherForecasts()
    {
      // Arrange
      var fetchWeatherForecastsRequest = new FetchWeatherForecastsAction();

      // Act
      WeatherForecastsState = await Mediator.Send(fetchWeatherForecastsRequest);

      // Assert
      WeatherForecastsState.WeatherForecasts.Count.ShouldBeGreaterThan(0);
    }
  }
}