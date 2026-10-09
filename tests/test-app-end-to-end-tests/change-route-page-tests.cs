#region Purpose
// E2E test: the Change Route page's button navigates to the home page.
#endregion

#region Design
// MSTest + Playwright PageTest against the running test app (Configuration.GetSutBaseUrl). Each test checks Server
// mode first, waits for the WASM bundle, reloads, and repeats in WebAssembly mode.
#endregion

namespace ChangeRoutePageTests;

[TestClass]
public class ChangeRouteTests : PageTest
{
  private string SutBaseUrl = null!;
  private ILocator ChangeRouteButtonLocator = null!;

  [TestInitialize]
  public async Task Initialize()
  {
    SutBaseUrl = Configuration.GetSutBaseUrl();

    // Navigate to the Change Route page
    await Page.GotoAsync($"{SutBaseUrl}/ChangeRoutePage");

    // Define the locators for the change route button
    ChangeRouteButtonLocator = Page.Locator("[data-qa='change-route-button']");
  }

  [TestMethod]
  public async Task TestChangeRoute()
  {
    // Validate Server Side
    await ValidateChangeRoute(RenderModes.Server);

    // Reload
    await PageUtilities.WaitTillBlazorWasmIsDownloadedAsync(Page);
    await Page.ReloadAsync();
    await Page.GotoAsync($"{SutBaseUrl}/ChangeRoutePage");
    
    // Validate in Wasm
    await ValidateChangeRoute(RenderModes.Wasm);
  }

  private async Task ValidateChangeRoute(string expectedCurrentMode)
  {
    await PageUtilities.ValidateRenderModesAsync(this, Page, expectedCurrentMode);

    // Click the change route button
    await ChangeRouteButtonLocator.ClickAsync();

    // Validate that the route changes to the home page
    await Expect(Page).ToHaveURLAsync($"{SutBaseUrl}/");
  }
}
