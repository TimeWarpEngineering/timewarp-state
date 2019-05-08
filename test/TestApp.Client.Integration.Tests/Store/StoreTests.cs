namespace TestApp.Client.Integration.Tests
{
  using System;
  using System.IO;
  using TestApp.Client.Features.Application;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Features.WeatherForecast;
  using BlazorState;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using TestApp.Client.Integration.Tests.Infrastructure;

  class StoreTests
  {
    public StoreTests(TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
      Store = ServiceProvider.GetService<IStore>();
      ReduxDevToolsStore = ServiceProvider.GetService<IReduxDevToolsStore>();
    }

    private IServiceProvider ServiceProvider { get; }
    private IStore Store { get; }
    private IReduxDevToolsStore ReduxDevToolsStore { get; }

    public void ShouldLoadStatesFromJson()
    {
      //Arrange
      string jsonString = File.ReadAllText("./Store/Store.json");
      //Act
      ReduxDevToolsStore.LoadStatesFromJson(jsonString);
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
    }
  }
}
