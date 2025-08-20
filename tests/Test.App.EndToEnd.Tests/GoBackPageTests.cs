namespace GoBackPageTests;

[TestClass]
public class GoBackTests : PageTest
{
  private string SutBaseUrl = null!;
  private ILocator GoBackButtonLocator = null!;
  private ILocator GoBackButton2Locator = null!;

  [TestInitialize]
  public async Task Initialize()
  {
    SutBaseUrl = Configuration.GetSutBaseUrl();
    await Page.GotoAsync($"{SutBaseUrl}/");

    // Define the locators for the go back buttons
    GoBackButtonLocator = Page.Locator("[data-qa='go-back-button']");
    GoBackButton2Locator = Page.Locator("[data-qa='go-back-button-2']");
  }

  [TestMethod]
  public async Task TestGoBack()
  {
    // Validate Server Side
    await ValidateGoBack(RenderModes.Server);

    // Reload
    await PageUtilities.WaitTillBlazorWasmIsDownloadedAsync(Page);
    await Page.ReloadAsync();

    // Validate in Wasm
    await ValidateGoBack(RenderModes.Wasm);
  }

  private async Task ValidateGoBack(string expectedCurrentMode)
  {
    // Start from the home page
    await Page.GotoAsync($"{SutBaseUrl}/");
    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Navigate through the app using links or programmatic navigation
    await Page.ClickAsync("a[href='/ChangeRoutePage']");
    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    await Page.ClickAsync("a[href='/CounterPage']");
    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    await Page.ClickAsync("a[href='/JavaScriptInteropPage']");
    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    await Page.ClickAsync("a[href='/GoBackPage']");
    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    await PageUtilities.ValidateRenderModesAsync(this, Page, expectedCurrentMode, ConfiguredRenderModes.InteractiveAutoRenderMode);

    // Click the go back button and validate the route
    await GoBackButtonLocator.ClickAsync();
    await Expect(Page).ToHaveURLAsync($"{SutBaseUrl}/JavaScriptInteropPage");

    // Navigate back to the Go Back page
    await Page.ClickAsync("a[href='/GoBackPage']");
    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Click the go back button 2 and validate the route
    await GoBackButton2Locator.ClickAsync();
    await Expect(Page).ToHaveURLAsync($"{SutBaseUrl}/CounterPage");
  }
}
