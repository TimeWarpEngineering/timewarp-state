namespace StaticWeatherForecastsPageTests;

[TestClass]
public class WeatherForecastsTests : PageTest
{
  private string SutBaseUrl = null!;
  private ILocator WeatherTableLocator = null!;

  [TestInitialize]
  public async Task Initialize()
  {
    SutBaseUrl = Configuration.GetSutBaseUrl() ?? throw new InvalidOperationException("SUT base URL is not configured.");

    // Navigate to the Weather Forecasts page
    await Page.GotoAsync($"{SutBaseUrl}/StaticWeatherForecastsPage");

    // Define the locators for the render modes and weather table
    WeatherTableLocator = Page.Locator("[data-qa='weather-table']");
  }

  [TestMethod]
  public async Task TestWeatherForecasts()
  {
    // Validate render modes
    await PageUtilities.ValidateRenderModesAsync(this, Page, RenderModes.Static, ConfiguredRenderModes.None);

    // Validate weather table
    await ValidateWeatherTable();
  }

  private async Task ValidateWeatherTable()
  {
    // Check that the weather table is visible
    await Expect(WeatherTableLocator).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 5000 });

    // Validate the content of the weather table
    ILocator tableRows = WeatherTableLocator.Locator("tr");
    await Expect(tableRows).ToHaveCountAsync(6); // 1 header row + 5 data rows
    
  }
}
