#region Purpose
// E2E smoke test: the home page renders statically with the expected title.
#endregion

#region Design
// Asserts the Static render mode with no configured mode, and logs browser name, version and user agent
// through TestContext (Console is banned, RS0030).
#endregion

namespace HomePage_;

[TestClass]
public class Should_ : PageTest
{
  [TestMethod]
  public async Task RenderStaticContent()
  {
    string sutBaseUrl = Configuration.GetSutBaseUrl() ?? throw new InvalidOperationException("SUT base URL is not configured. Please check your test configuration.");
    await Page.GotoAsync(sutBaseUrl);
    TestContext.WriteLine($"Browser: {Page.Context.Browser?.BrowserType.Name}");
    TestContext.WriteLine($"Browser Version: {Page.Context.Browser?.Version}");
    TestContext.WriteLine($"User Agent: {await Page.EvaluateAsync<string>("() => navigator.userAgent")}");

    await Expect(Page).ToHaveTitleAsync("TimeWarp.State Test App");
    await PageUtilities.ValidateRenderModesAsync(this, Page, RenderModes.Static, ConfiguredRenderModes.None);
  }
}
