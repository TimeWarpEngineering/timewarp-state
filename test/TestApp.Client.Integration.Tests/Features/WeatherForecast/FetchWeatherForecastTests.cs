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
      TestFixture aTestFixture,
      BlazorStateTestServer aBlazorStateTestServer)
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
      // Default WeatherForecastsState is an empty list. So no need to initialize it.
      // We need the server running to respond.
      // In Memory one hopefully works here with blazor.
      // will need a way to provide it the WebClient to use.

      var fetchWeatherForecastsRequest = new FetchWeatherForecastsRequest();

      WeatherForecastsState = await Mediator.Send(fetchWeatherForecastsRequest);

      WeatherForecastsState.WeatherForecasts.Count.ShouldBeGreaterThan(0);
    }
  }
}