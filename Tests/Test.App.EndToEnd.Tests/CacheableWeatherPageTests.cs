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
    Console.WriteLine("Starting TestCacheableWeather");

    // Validate initial state
    Console.WriteLine("Step 1: Validating initial state");
    await ValidateInitialState();
    Console.WriteLine("Initial state validated");

    // Click the button to fetch weather forecasts
    Console.WriteLine("Step 2: Clicking button to fetch weather forecasts");
    await FetchWeatherForecastsButtonLocator.ClickAsync();
    Console.WriteLine("Button clicked");

    // Validate weather forecasts and cache state
    Console.WriteLine("Step 3: Validating weather forecasts and cache state (first fetch)");
    await ValidateWeatherForecastsAndCacheState(true);
    Console.WriteLine("First fetch validated");

    // Click the button a second time within the CacheDuration
    Console.WriteLine("Step 4: Clicking button a second time within CacheDuration");
    await FetchWeatherForecastsButtonLocator.ClickAsync();
    Console.WriteLine("Button clicked second time");

    // Validate weather forecasts and cache state again
    Console.WriteLine("Step 5: Validating weather forecasts and cache state (second fetch)");
    await ValidateWeatherForecastsAndCacheState(true);
    Console.WriteLine("Second fetch validated");

    // Wait longer than cache duration and then click the button a third time
    Console.WriteLine("Step 6: Waiting for cache duration to expire");
    await Task.Delay(TimeSpan.FromSeconds(11));
    Console.WriteLine("Wait completed");

    Console.WriteLine("Step 7: Clicking button a third time after cache expiration");
    await FetchWeatherForecastsButtonLocator.ClickAsync();
    Console.WriteLine("Button clicked third time");

    // Validate new weather forecasts and cache state
    Console.WriteLine("Step 8: Validating new weather forecasts and cache state");
    await ValidateWeatherForecastsAndCacheState(false);
    Console.WriteLine("Third fetch validated");

    Console.WriteLine("TestCacheableWeather completed successfully");
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

    const string cacheKey = "Test.App.Client.Features.WeatherForecast.CacheableWeatherState+FetchWeatherForecastsActionSet+Action|{}";
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
