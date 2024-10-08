namespace Test.App.EndToEnd.Tests;
using Microsoft.Playwright;

public static class PageUtilities
{
  private static ILocator CurrentRenderModeLocator(IPage page)
  {
    return page.Locator("[data-qa='current-render-mode']");
  }

  private static ILocator ConfiguredRenderModeLocator(IPage page)
  {
    return page.Locator("[data-qa='configured-render-mode']");
  }

  public static async Task WaitTillBlazorWasmIsDownloadedAsync(IPage page)
  {
    const int retries = 10;
    const int delay = 1000; // 1 second

    for (int i = 0; i < retries; i++)
    {
      // Evaluate JavaScript to check for the key in local storage
      bool isBlazorDownloaded = await page.EvaluateAsync<bool>(@"
      () => {
        for (let i = 0; i < localStorage.length; i++) {
          if (localStorage.key(i).includes('blazor-resource-hash:')) {
            return true;
          }
        }
        return false;
      }
    ");

      if (isBlazorDownloaded)
      {
        Console.WriteLine("Blazor WASM resource is downloaded.");
        return;
      }

      await Task.Delay(delay);
    }

    throw new Exception("Blazor WASM resource was not downloaded within the expected time.");
  }

  
  public static async Task ValidateRenderModesAsync(PlaywrightTest test, IPage page, string expectedCurrentMode, string expectedConfiguredMode)
  {
    ILocator currentRenderModeLocator = page.Locator("[data-qa='current-render-mode']");
    ILocator configuredRenderModeLocator = page.Locator("[data-qa='configured-render-mode']");

    // Now validate both current and configured modes
    await test.Expect(currentRenderModeLocator).ToHaveTextAsync(expectedCurrentMode, new() { Timeout = 10000 });
    await test.Expect(configuredRenderModeLocator).ToHaveTextAsync(expectedConfiguredMode);
  }
}
