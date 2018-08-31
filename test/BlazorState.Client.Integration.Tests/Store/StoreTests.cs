namespace BlazorState.Client.Integration.Tests
{
  using System;
  using System.IO;
  using BlazorState.Client.Features.Application;
  using BlazorState.Client.Features.Counter;
  using BlazorState.Client.Features.WeatherForecast;
  using BlazorState.Features.Routing;
  using BlazorState.Integration.Tests.Infrastructure;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;

  class StoreTests
  {
    public StoreTests(TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
      Store = ServiceProvider.GetService<IStore>() as Store;
      CounterState = Store.GetState<CounterState>();
      ApplicationState = Store.GetState<ApplicationState>();
      WeatherForecastState = Store.GetState<WeatherForecastsState>();
      RouteState = Store.GetState<RouteState>();
    }

    private CounterState CounterState { get; set; }
    private ApplicationState ApplicationState { get; set; }
    private WeatherForecastsState WeatherForecastState { get; set; }
    private RouteState RouteState { get; set; }

    private IServiceProvider ServiceProvider { get; }
    private Store Store { get; }

#if DEBUG
    public void ShouldLoadStatesFromJson()
    {
      //Arrange
      string jsonString = File.ReadAllText("./Store/Store.json");
      //Act
      Store.LoadStatesFromJson(jsonString);
      // Assert
      ApplicationState applicationState = Store.GetState<ApplicationState>();
      applicationState.Name.ShouldBe("Blazor State Demo Application");
      applicationState.Guid.ToString().ShouldBe("5a2efcec-6297-4254-a2dc-30e4e567e549");

      CounterState counterState = Store.GetState<CounterState>();
      counterState.Count.ShouldBe(18);
      counterState.Guid.ToString().ShouldBe("a0d74c63-13f4-4a2f-b18b-9a1fdaa397b2");

      WeatherForecastsState weatherForecastsState = Store.GetState<WeatherForecastsState>();
      weatherForecastsState.WeatherForecasts.Count.ShouldBe(5);
      weatherForecastsState.WeatherForecasts[0].Summary.ShouldBe("Freezing");
      weatherForecastsState.WeatherForecasts[0].TemperatureC.ShouldBe(16);
      weatherForecastsState.WeatherForecasts[0].TemperatureF.ShouldBe(60);
      weatherForecastsState.WeatherForecasts[0].Date.Year.ShouldBe(2018);
      weatherForecastsState.WeatherForecasts[0].Date.Month.ShouldBe(8);
      weatherForecastsState.WeatherForecasts[0].Date.Day.ShouldBe(26);
      weatherForecastsState.WeatherForecasts[0].Date.Hour.ShouldBe(9);
      weatherForecastsState.WeatherForecasts[0].Date.Minute.ShouldBe(29);
      weatherForecastsState.WeatherForecasts[0].Date.Second.ShouldBe(54);

      RouteState routeState = Store.GetState<RouteState>();
      routeState.Route.ShouldBe("http://localhost:54956/fetchdata");
      routeState.Guid.ToString().ShouldBe("b2bb7499-d8b1-4deb-a664-5c67b51d8434");
    }
#endif
  }
}
