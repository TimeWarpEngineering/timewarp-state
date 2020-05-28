namespace WeatherForecastsState
{
  using Shouldly;
  using System.Threading.Tasks;
  using TestApp.Client.Features.WeatherForecast;
  using TestApp.Client.Integration.Tests.Infrastructure;
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
      WeatherForecastsState.WeatherForecasts.Count.ShouldBeGreaterThan(0);
    }
  }
}