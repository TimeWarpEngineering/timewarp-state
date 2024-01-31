namespace WeatherForecastsState_;

using static TestApp.Client.Features.WeatherForecast.WeatherForecastsState;

public class FetchWeatherForecastsAction_Should : BaseTest
{
  private WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();

  public FetchWeatherForecastsAction_Should(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost) { }

  public async Task Should_Fetch_WeatherForecasts()
  {
    // Arrange
    var fetchWeatherForecastsRequest = new FetchWeatherForecastsAction();

    // Act
    await Send(fetchWeatherForecastsRequest);

    // Assert
    WeatherForecastsState.WeatherForecasts.Count.Should().BeGreaterThan(0);
  }
}
