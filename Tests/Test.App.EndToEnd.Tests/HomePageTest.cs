namespace HomePage_;

[TestClass]
public class Should_ : PageTest
{
  [TestMethod]
  public async Task RenderStaticContent()
  {
    string sutBaseUrl = Configuration.GetSutBaseUrl();
    await Page.GotoAsync(sutBaseUrl);
    Console.WriteLine($"Browser: {Page.Context.Browser.BrowserType.Name}");
    Console.WriteLine($"Browser Version: {Page.Context.Browser.Version}");
    Console.WriteLine($"User Agent: {await Page.EvaluateAsync<string>("() => navigator.userAgent")}");

    await Expect(Page).ToHaveTitleAsync("TimeWarp.State Test App");
    await PageUtilities.ValidateRenderModesAsync(this, Page, RenderModes.Static, ConfiguredRenderModes.None);
  }
}
