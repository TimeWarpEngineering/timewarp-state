namespace CacheableWeatherPageTests;

[TestClass]
public class CacheableWeatherTests : PageTest
{
  private string SutBaseUrl = null!;
  private ILocator CacheKeyLocator = null!;
  private ILocator CacheDurationLocator = null!;
  private ILocator TimeStampLocator = null!;
  private ILocator FetchWeatherForecastsButtonLocator = null!;
  private ILocator WeatherTableLocator = null!;

  [TestInitialize]
  public async Task Initialize()
  {
    SutBaseUrl = Configuration.GetSutBaseUrl() ?? throw new InvalidOperationException("SUT base URL is not configured.");

    // Navigate to the Cacheable Weather page
    await Page.GotoAsync($"{SutBaseUrl}/CacheableWeatherForecastsPage");

    // Define the locators for the render modes, cache information, and weather table
    CacheKeyLocator = Page.Locator("[data-qa='cache-key']");
    CacheDurationLocator = Page.Locator("[data-qa='cache-duration']");
    TimeStampLocator = Page.Locator("[data-qa='timestamp']");
    FetchWeatherForecastsButtonLocator = Page.Locator("[data-qa='fetch-weather-forecasts']");
    WeatherTableLocator = Page.Locator("[data-qa='weather-table']");
  }

  [TestMethod]
  public async Task TestCacheableWeather()
  {
    // Validate initial state
    await ValidateInitialState();

    // Click the button to fetch weather forecasts
    await FetchWeatherForecastsButtonLocator.ClickAsync();

    // Validate weather forecasts and cache state
    await ValidateWeatherForecastsAndCacheState(true);

    // Click the button a second time within the CacheDuration
    await FetchWeatherForecastsButtonLocator.ClickAsync();

    // Validate weather forecasts and cache state again
    await ValidateWeatherForecastsAndCacheState(true);

    // Wait longer than cache duration and then click the button a third time
    await Task.Delay(TimeSpan.FromSeconds(11));
    await FetchWeatherForecastsButtonLocator.ClickAsync();

    // Validate new weather forecasts and cache state
    await ValidateWeatherForecastsAndCacheState(false);
  }

  private async Task ValidateInitialState()
  {
    // Validate render modes
    await PageUtilities.ValidateRenderModesAsync(this, Page, RenderModes.Wasm, ConfiguredRenderModes.InteractiveWebAssemblyRenderMode);

    // Validate initial state of cache and weather forecasts
    await Expect(CacheKeyLocator).ToBeHiddenAsync();
    await Expect(TimeStampLocator).ToBeHiddenAsync();
    await Expect(WeatherTableLocator).ToBeHiddenAsync();
    await Expect(CacheDurationLocator).ToHaveTextAsync("00:00:10");
  }

  private async Task ValidateWeatherForecastsAndCacheState(bool isCached, string? previousTimeStamp = null)
  {
    // Validate the weather table
    await Expect(WeatherTableLocator).ToBeVisibleAsync();

    // Validate the cache key
    const string cacheKey = "Test.App.Client.Features.WeatherForecast.CacheableWeatherState+FetchWeatherForecasts+Action|{}";
    await Expect(CacheKeyLocator).ToHaveTextAsync(cacheKey);

    // Validate the timestamp
    string currentTimestamp = await TimeStampLocator.TextContentAsync() ?? throw new InvalidOperationException("Timestamp is null.");
    if (isCached)
    {
      if (previousTimeStamp != null)
      {
        await Expect(TimeStampLocator).ToHaveTextAsync(previousTimeStamp);
      }
    }
    else
    {
      if (previousTimeStamp != null)
      {
        await Expect(TimeStampLocator).Not.ToHaveTextAsync(previousTimeStamp);
      }
    }
  }
}
