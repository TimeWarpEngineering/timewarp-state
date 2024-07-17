namespace CounterPageTests;

[TestClass]
public class CounterTests : PageTest
{
  private string SutBaseUrl = null!;
  private ILocator Counter1Locator = null!;
  private ILocator Counter2Locator = null!;
  private ILocator Counter1ButtonLocator = null!;
  private ILocator Counter2ButtonLocator = null!;

  [TestInitialize]
  public async Task Initialize()
  {
    SutBaseUrl = Configuration.GetSutBaseUrl();

    // Navigate to the Counter page
    await Page.GotoAsync($"{SutBaseUrl}/CounterPage");

    // Define the locators for the render modes and counters
    Counter1Locator = Page.Locator("[data-qa='Counter1'] [data-qa='count']");
    Counter2Locator = Page.Locator("[data-qa='Counter2'] [data-qa='count']");
    Counter1ButtonLocator = Page.Locator("[data-qa='Counter1'] button");
    Counter2ButtonLocator = Page.Locator("[data-qa='Counter2'] button");
  }

  [TestMethod]
  public async Task TestCounterComponents()
  {
    await TestRenderModeAndCountersAsync(RenderModes.Server);
    await PageUtilities.WaitTillBlazorWasmIsDownloadedAsync(Page);
    await Page.ReloadAsync();
    await TestRenderModeAndCountersAsync(RenderModes.Wasm);
  }

  private async Task TestRenderModeAndCountersAsync(string expectedCurrentMode)
  {
    await PageUtilities.ValidateRenderModesAsync(this, Page, expectedCurrentMode, ConfiguredRenderModes.InteractiveAutoRenderMode);
    await ValidateCountersAsync("3");
    await Counter1ButtonLocator.ClickAsync();
    await ValidateCountersAsync("8");
    await Counter2ButtonLocator.ClickAsync();
    await ValidateCountersAsync("13");
  }

  private async Task ValidateCountersAsync(string expectedCount)
  {
    await Expect(Counter1Locator).ToHaveTextAsync(expectedCount);
    await Expect(Counter2Locator).ToHaveTextAsync(expectedCount);
  }
}
