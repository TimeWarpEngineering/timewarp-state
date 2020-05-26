namespace TestApp.Client.Integration.Tests.Features.WeatherForecast_Tests
{
  using Shouldly;
  using System.Threading.Tasks;
  using TestApp.Client.Features.WeatherForecast;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using static TestApp.Client.Features.WeatherForecast.WeatherForecastsState;

  internal class FetchWeatherForecastTests : BaseTest
  {
    private WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();

    public FetchWeatherForecastTests(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost) { }

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