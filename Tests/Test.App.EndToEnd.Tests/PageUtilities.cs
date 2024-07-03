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

  public static async Task WaitForLocalStorageNotEmptyAsync(IPage page)
  {
    const int retries = 10;
    const int delay = 1000; // 1 second

    for (int i = 0; i < retries; i++)
    {
      string localStorageContent = await page.EvaluateAsync<string>("() => JSON.stringify(window.localStorage)");
      if (!string.IsNullOrEmpty(localStorageContent) && localStorageContent != "{}")
      {
        Console.WriteLine($"Local Storage is populated: {localStorageContent}");
        return;
      }

      await Task.Delay(delay);
    }

    throw new Exception("Local storage is still empty after waiting.");
  }
}
