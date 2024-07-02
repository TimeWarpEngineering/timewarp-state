namespace HomePage_;

using Microsoft.Playwright.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class Should_ : PageTest
{
  [TestMethod]
  public async Task HaveTitle()
  {
    string sutBaseUrl = Configuration.GetSutBaseUrl();
    await Page.GotoAsync(sutBaseUrl);
        
    await Expect(Page).ToHaveTitleAsync("TimeWarp.State Test App");
       
    // Validate Current Render Mode
    ILocator currentRenderModeLocator = Page.Locator("[data-qa='current-render-mode']");
    await Expect(currentRenderModeLocator).ToHaveTextAsync("Current Render Mode: Static");

    // Validate Configured Render Mode
    ILocator configuredRenderModeLocator = Page.Locator("[data-qa='configured-render-mode']");
    await Expect(configuredRenderModeLocator).ToHaveTextAsync("Configured Render Mode: None");
  }
}
