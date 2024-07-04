namespace ThrowExceptionPageTests;

[TestClass]
public class ThrowExceptionTests : PageTest
{
  private string SutBaseUrl = null!;
  private ILocator CounterStateGuidLocator = null!;
  private ILocator ThrowClientSideExceptionButtonLocator = null!;
  private ILocator ThrowServerSideExceptionButtonLocator = null!;

  [TestInitialize]
  public async Task Initialize()
  {
    SutBaseUrl = Configuration.GetSutBaseUrl();

    // Navigate to the Exception page
    await Page.GotoAsync($"{SutBaseUrl}/ThrowExceptionPage");

    // Define the locators for the render modes and counter state
    CounterStateGuidLocator = Page.Locator("[data-qa='counter-state-guid']");
    ThrowClientSideExceptionButtonLocator = Page.Locator("[data-qa='throw-client-side-exception']");
    ThrowServerSideExceptionButtonLocator = Page.Locator("[data-qa='throw-server-side-exception']");
  }

  [TestMethod]
  public async Task TestThrowException()
  {
    // Validate Server Side
    await ValidateExceptionHandling(RenderModes.Server);

    // Reload
    await PageUtilities.WaitTillBlazorWasmIsDownloadedAsync(Page);
    await Page.ReloadAsync();
    
    // Validate in Wasm
    await ValidateExceptionHandling(RenderModes.Wasm);
  }

  private async Task ValidateExceptionHandling(string expectedCurrentMode)
  {
    await PageUtilities.ValidateRenderModesAsync(this, Page, expectedCurrentMode, ConfiguredRenderModes.InteractiveAutoRenderMode);

    string? initialGuid = await CounterStateGuidLocator.TextContentAsync();
    initialGuid.Should().NotBeNull();
    
    // Throw client-side exception and validate
    await ThrowClientSideExceptionButtonLocator.ClickAsync();
    await Expect(CounterStateGuidLocator).ToHaveTextAsync(initialGuid!);

    // Throw server-side exception and validate
    await ThrowServerSideExceptionButtonLocator.ClickAsync();
    await Expect(CounterStateGuidLocator).ToHaveTextAsync(initialGuid!);
  }
}
