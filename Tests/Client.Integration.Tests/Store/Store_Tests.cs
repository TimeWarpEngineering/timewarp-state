namespace TestApp.Client.Integration.Tests;

internal class Store_Tests : BaseTest
{
#if ReduxDevToolsEnabled
  private readonly IReduxDevToolsStore ReduxDevToolsStore;
#endif
  public Store_Tests(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost)
  {
#if ReduxDevToolsEnabled
    ReduxDevToolsStore = ServiceProvider.GetService<IReduxDevToolsStore>();
#endif
  }

#if ReduxDevToolsEnabled
  public void ShouldLoadStatesFromJson()
  {
    //Arrange
    string jsonString = File.ReadAllText("./Store/Store.json");
    //Act
    ReduxDevToolsStore.LoadStatesFromJson(jsonString);
    // Assert
    ApplicationState applicationState = Store.GetState<ApplicationState>();
    applicationState.Name.Should().Be("Blazor State Demo");
    applicationState.Guid.ToString().Should().Be("5a2efcec-6297-4254-a2dc-30e4e567e549");

    CounterState counterState = Store.GetState<CounterState>();
    counterState.Count.Should().Be(18);
    counterState.Guid.ToString().Should().Be("a0d74c63-13f4-4a2f-b18b-9a1fdaa397b2");

    WeatherForecastsState weatherForecastsState = Store.GetState<WeatherForecastsState>();
    weatherForecastsState.WeatherForecasts.Count.Should().Be(5);
    weatherForecastsState.WeatherForecasts[0].Summary.Should().Be("Freezing");
    weatherForecastsState.WeatherForecasts[0].TemperatureC.Should().Be(16);
    weatherForecastsState.WeatherForecasts[0].TemperatureF.Should().Be(60);
    weatherForecastsState.WeatherForecasts[0].Date.Year.Should().Be(2018);
    weatherForecastsState.WeatherForecasts[0].Date.Month.Should().Be(8);
    weatherForecastsState.WeatherForecasts[0].Date.Day.Should().Be(26);
    weatherForecastsState.WeatherForecasts[0].Date.Hour.Should().Be(9);
    weatherForecastsState.WeatherForecasts[0].Date.Minute.Should().Be(29);
    weatherForecastsState.WeatherForecasts[0].Date.Second.Should().Be(54);
  }
#endif

  /// <summary>
  /// WeatherForecatesState will throw an exception if items initialized in the constructor are null.
  /// </summary>
  public void ShouldInitializeStateAfterConstruction()
  {
    WeatherForecastsState state = Store.GetState<WeatherForecastsState>();
    state.Should().NotBeNull();
  }
}
