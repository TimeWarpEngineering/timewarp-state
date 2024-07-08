namespace HomePage_;

[TestClass]
public class Should_ : PageTest
{
  [TestMethod]
  public async Task RenderStaticContent()
  {
    string sutBaseUrl = Configuration.GetSutBaseUrl();
    await Page.GotoAsync(sutBaseUrl);
    await Expect(Page).ToHaveTitleAsync("TimeWarp.State Test App");
    await PageUtilities.ValidateRenderModesAsync(this, Page, RenderModes.Static, ConfiguredRenderModes.None);
  }
}
