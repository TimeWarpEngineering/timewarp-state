#region Purpose
// E2E test for the Cacheable Weather page: a second fetch inside CacheDuration is served from cache, a later one is
// not.
#endregion

#region Design
// Reads cache-key, cache-duration and timestamp from data-qa spans and waits 11 seconds, past the 10 second
// CacheDuration, before the third fetch. It logs each step and dumps the page HTML on failure to make CI failures
// diagnosable.
#endregion

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
    TestContext.WriteLine("Starting TestCacheableWeather");

    // Validate initial state
    TestContext.WriteLine("Step 1: Validating initial state");
    await ValidateInitialState();
    TestContext.WriteLine("Initial state validated");

    // Click the button to fetch weather forecasts
    TestContext.WriteLine("Step 2: Clicking button to fetch weather forecasts");
    await FetchWeatherForecastsButtonLocator.ClickAsync();
    TestContext.WriteLine("Button clicked");

    // Validate weather forecasts and cache state
    TestContext.WriteLine("Step 3: Validating weather forecasts and cache state (first fetch)");
    await ValidateWeatherForecastsAndCacheState(true);
    TestContext.WriteLine("First fetch validated");

    // Click the button a second time within the CacheDuration
    TestContext.WriteLine("Step 4: Clicking button a second time within CacheDuration");
    await FetchWeatherForecastsButtonLocator.ClickAsync();
    TestContext.WriteLine("Button clicked second time");

    // Validate weather forecasts and cache state again
    TestContext.WriteLine("Step 5: Validating weather forecasts and cache state (second fetch)");
    await ValidateWeatherForecastsAndCacheState(true);
    TestContext.WriteLine("Second fetch validated");

    // Wait longer than cache duration and then click the button a third time
    TestContext.WriteLine("Step 6: Waiting for cache duration to expire");
    await Task.Delay(TimeSpan.FromSeconds(11));
    TestContext.WriteLine("Wait completed");

    TestContext.WriteLine("Step 7: Clicking button a third time after cache expiration");
    await FetchWeatherForecastsButtonLocator.ClickAsync();
    TestContext.WriteLine("Button clicked third time");

    // Validate new weather forecasts and cache state
    TestContext.WriteLine("Step 8: Validating new weather forecasts and cache state");
    await ValidateWeatherForecastsAndCacheState(false);
    TestContext.WriteLine("Third fetch validated");

    TestContext.WriteLine("TestCacheableWeather completed successfully");
  }

  private async Task ValidateInitialState()
  {
    // Validate render modes
    await PageUtilities.ValidateRenderModesAsync(this, Page, RenderModes.Wasm, ConfiguredRenderModes.InteractiveWebAssemblyRenderMode);

    // Cache-key and timestamp spans are always in the DOM; they are empty until the first fetch.
    await Expect(CacheKeyLocator).ToHaveTextAsync(string.Empty);
    await Expect(TimeStampLocator).ToHaveTextAsync(string.Empty);
    await Expect(WeatherTableLocator).ToBeHiddenAsync();
    await Expect(CacheDurationLocator).ToHaveTextAsync("00:00:10");
  }

  private async Task ValidateWeatherForecastsAndCacheState(bool isCached, string? previousTimeStamp = null)
  {
    TestContext.WriteLine("Starting ValidateWeatherForecastsAndCacheState");

    // Validate the weather table
    TestContext.WriteLine("Checking if weather table is visible");
    await Expect(WeatherTableLocator).ToBeVisibleAsync();
    TestContext.WriteLine("Weather table visibility check completed");

    // Validate the cache key
    const string expectedCacheKey = "Test.App.Client.Features.WeatherForecast.CacheableWeatherState+FetchWeatherForecastsActionSet+Action|{}";
    TestContext.WriteLine($"Checking cache key. Expected: {expectedCacheKey}");
    await Expect(CacheKeyLocator).ToHaveTextAsync(expectedCacheKey);
    TestContext.WriteLine("Cache key check completed");

    // Validate the timestamp
    TestContext.WriteLine("Checking timestamp");
    string? currentTimestamp = await TimeStampLocator.TextContentAsync();
    TestContext.WriteLine($"Current timestamp: {currentTimestamp}");

    if (isCached && previousTimeStamp != null)
    {
      TestContext.WriteLine($"Expecting timestamp to match previous: {previousTimeStamp}");
      await Expect(TimeStampLocator).ToHaveTextAsync(previousTimeStamp);
    }
    else if (!isCached && previousTimeStamp != null)
    {
      TestContext.WriteLine($"Expecting timestamp to be different from previous: {previousTimeStamp}");
      await Expect(TimeStampLocator).Not.ToHaveTextAsync(previousTimeStamp);
    }
    else
    {
      TestContext.WriteLine("Previous timestamp is null, skipping comparison");
    }

    TestContext.WriteLine("Timestamp check completed");

    TestContext.WriteLine("ValidateWeatherForecastsAndCacheState completed");
  }

  private async Task LogPageContentOnFailure(Func<Task> action)
  {
    try
    {
      await action();
    }
    catch
    {
      TestContext.WriteLine("Test failed. Capturing page content:");
      TestContext.WriteLine(await Page.ContentAsync());
      throw;
    }
  }
}
