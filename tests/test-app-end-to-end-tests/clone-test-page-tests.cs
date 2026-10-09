#region Purpose
// Runs the deep-clone suite (CloneProviderTests) inside the app in Server and real-browser WebAssembly mode.
#endregion

#region Design
// .NET 11 throws PlatformNotSupportedException for blocking waits on single-threaded browser WASM; the old
// AnyClone/TypeSupport path hit SemaphoreSlim.Wait. Server-side tests cannot see that, so this runs in the browser.
#endregion

namespace CloneTestPageTests;

[TestClass]
public class CloneTestPageTests : PageTest
{
  private string SutBaseUrl = null!;
  private ILocator ResultLocator = null!;
  private ILocator RunButtonLocator = null!;

  [TestInitialize]
  public async Task Initialize()
  {
    SutBaseUrl = Configuration.GetSutBaseUrl();

    await Page.GotoAsync($"{SutBaseUrl}/cloneTest");

    ResultLocator = Page.Locator("[data-qa='clone-test-result']");
    RunButtonLocator = Page.Locator("[data-qa='run-clone-test']");
  }

  [TestMethod]
  public async Task CloneSuitePassesInServerAndWasm()
  {
    await RunCloneSuiteAsync(RenderModes.Server);
    await PageUtilities.WaitTillBlazorWasmIsDownloadedAsync(Page);
    await Page.ReloadAsync();
    await RunCloneSuiteAsync(RenderModes.Wasm);
  }

  private async Task RunCloneSuiteAsync(string expectedCurrentMode)
  {
    await PageUtilities.ValidateRenderModesAsync(this, Page, expectedCurrentMode);
    await Expect(ResultLocator).ToHaveTextAsync("Not run");
    await RunButtonLocator.ClickAsync();
    await Expect(ResultLocator).ToHaveTextAsync("Passed");
  }
}
