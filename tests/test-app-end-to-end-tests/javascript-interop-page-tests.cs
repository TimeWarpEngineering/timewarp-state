namespace JavaScriptInteropPageTests;

[TestClass]
public class JavaScriptInteropTests : PageTest
{
  private string SutBaseUrl = null!;
  private ILocator CounterStateCountLocator = null!;
  private ILocator IncrementButtonLocator = null!;

  [TestInitialize]
  public async Task Initialize()
  {
    SutBaseUrl = Configuration.GetSutBaseUrl();

    // Navigate to the JavaScript Interop page
    await Page.GotoAsync($"{SutBaseUrl}/JavaScriptInteropPage");

    // Define the locators for the render modes and counter state
    CounterStateCountLocator = Page.Locator("[data-qa='counter-state-count']");
    IncrementButtonLocator = Page.Locator("[data-qa='increment-button']");
  }

  [TestMethod]
  public async Task TestJavaScriptInterop()
  {
    // Validate Server Side
    await ValidateJavaScript(RenderModes.Server);

    // Reload
    await PageUtilities.WaitTillBlazorWasmIsDownloadedAsync(Page);
    await Page.ReloadAsync();
    
    // Validate in Wasm
    await ValidateJavaScript(RenderModes.Wasm);
  }
  private async Task ValidateJavaScript(string expectedCurrentMode)
  {
    await PageUtilities.ValidateRenderModesAsync(this, Page, expectedCurrentMode, ConfiguredRenderModes.InteractiveAutoRenderMode);
    await ValidateCounterStateAsync("3");
    await IncrementButtonLocator.ClickAsync();
    await ValidateCounterStateAsync("10");
  }

  private async Task ValidateCounterStateAsync(string expectedCount)
  {
    await Expect(CounterStateCountLocator).ToHaveTextAsync(expectedCount);
  }
}
