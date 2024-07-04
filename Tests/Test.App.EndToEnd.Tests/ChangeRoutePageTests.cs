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
    await PageUtilities.WaitForLocalStorageNotEmptyAsync(Page);
    await Page.ReloadAsync();
    await Page.GotoAsync($"{SutBaseUrl}/ChangeRoutePage");
    
    // Validate in Wasm
    await ValidateChangeRoute(RenderModes.Wasm);
  }

  private async Task ValidateChangeRoute(string expectedCurrentMode)
  {
    await PageUtilities.ValidateRenderModesAsync(this, Page, expectedCurrentMode, ConfiguredRenderModes.InteractiveAutoRenderMode);

    // Click the change route button
    await ChangeRouteButtonLocator.ClickAsync();

    // Validate that the route changes to the home page
    await Expect(Page).ToHaveURLAsync($"{SutBaseUrl}/");
  }
}
