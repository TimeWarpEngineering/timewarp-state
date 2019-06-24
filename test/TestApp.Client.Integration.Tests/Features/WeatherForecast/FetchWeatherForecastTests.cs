namespace TestApp.Client.Integration.Tests.Features.WeatherForecast
{
  using System;
  using System.Threading.Tasks;
  using TestApp.Client.Features.WeatherForecast;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;

  internal class FetchWeatherForecastTests
  {
    public FetchWeatherForecastTests(
      TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
      Mediator = ServiceProvider.GetService<IMediator>();
      Store = ServiceProvider.GetService<IStore>();
      WeatherForecastsState = Store.GetState<WeatherForecastsState>();
    }

    private IMediator Mediator { get; }
    private IServiceProvider ServiceProvider { get; }
    private IStore Store { get; }
    private WeatherForecastsState WeatherForecastsState { get; set; }

    public async Task Should_Fetch_WeatherForecasts()
    {
      // Arrange
      // Default WeatherForecastsState is an empty list. So no need to initialize it.
      var fetchWeatherForecastsRequest = new FetchWeatherForecastsAction();

      // Act
      WeatherForecastsState = await Mediator.Send(fetchWeatherForecastsRequest);

      // Assert
      WeatherForecastsState.WeatherForecasts.Count.ShouldBeGreaterThan(0);
    }
  }
}