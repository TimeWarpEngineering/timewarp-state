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
  using static TestApp.Client.Features.WeatherForecast.WeatherForecastsState;

  internal class FetchWeatherForecastTests
  {
    private readonly IMediator Mediator;
    private readonly IStore Store;

    private WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();

    public FetchWeatherForecastTests(TestFixture aTestFixture)
    {
      IServiceProvider serviceProvider = aTestFixture.ServiceProvider;
      Mediator = serviceProvider.GetService<IMediator>();
      Store = serviceProvider.GetService<IStore>();
    }

    public async Task Should_Fetch_WeatherForecasts()
    {
      // Arrange
      var fetchWeatherForecastsRequest = new FetchWeatherForecastsAction();

      // Act
      _ = await Mediator.Send(fetchWeatherForecastsRequest);

      // Assert
      WeatherForecastsState.WeatherForecasts.Count.ShouldBeGreaterThan(0);
    }
  }
}