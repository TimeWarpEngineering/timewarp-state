namespace ResetStorePageTests;

[TestClass]
public class ResetStoreTests : PageTest
{
  private string SutBaseUrl = null!;
  private ILocator ResetStoreButtonLocator = null!;

  [TestInitialize]
  public async Task Initialize()
  {
    SutBaseUrl = Configuration.GetSutBaseUrl();

    // Navigate to the Reset Store page
    await Page.GotoAsync($"{SutBaseUrl}/ResetStorePage");

    // Define the locators for the reset store button
    ResetStoreButtonLocator = Page.Locator("[data-qa='reset-store-button']");
  }

  [TestMethod]
  public async Task TestResetStore()
  {
    // Validate Server Side
    await ValidateResetStore(RenderModes.Server);
    
    // Reload
    await PageUtilities.WaitForLocalStorageNotEmptyAsync(Page);
    await Page.ReloadAsync();
    await Page.GotoAsync($"{SutBaseUrl}/ResetStorePage");
    
    // Validate in Wasm
    await ValidateResetStore(RenderModes.Wasm);
  }

  private async Task ValidateResetStore(string expectedCurrentMode)
  {
    await PageUtilities.ValidateRenderModesAsync(this, Page, expectedCurrentMode, ConfiguredRenderModes.InteractiveAutoRenderMode);

    // Click the reset store button
    await ResetStoreButtonLocator.ClickAsync();

    // Validate that the route changes to the home page
    await Expect(Page).ToHaveURLAsync($"{SutBaseUrl}/");
  }
}
