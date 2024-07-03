namespace CounterPageTests;

[TestClass]
public class CounterTests : PageTest
{
    [TestMethod]
    public async Task TestCounterComponents()
    {
        // Navigate to the Counter page
        string sutBaseUrl = Configuration.GetSutBaseUrl();
        
        // Navigate to the Counter page
        await Page.GotoAsync($"{sutBaseUrl}/counter");
        
        // Validate Current Render Mode
        ILocator currentRenderModeLocator = Page.Locator("[data-qa='current-render-mode']");
        await Expect(currentRenderModeLocator).ToHaveTextAsync("Current Render Mode: Server");

        // Validate Configured Render Mode
        ILocator configuredRenderModeLocator = Page.Locator("[data-qa='configured-render-mode']");
        await Expect(configuredRenderModeLocator).ToHaveTextAsync("Configured Render Mode: InteractiveAutoRenderMode");

        // Define the locators for the counters
        ILocator counter1Locator = Page.Locator("[data-qa='Counter1'] [data-qa='count']");
        ILocator counter2Locator = Page.Locator("[data-qa='Counter2'] [data-qa='count']");
        
        // Initial assertions
        await Expect(counter1Locator).ToHaveTextAsync("CounterState.Count: 3");
        await Expect(counter2Locator).ToHaveTextAsync("CounterState.Count: 3");

        // Click the Counter1 button and verify
        await Page.ClickAsync("[data-qa='Counter1'] button");
        await Expect(counter1Locator).ToHaveTextAsync("CounterState.Count: 8");
        await Expect(counter2Locator).ToHaveTextAsync("CounterState.Count: 8");

        // Click the Counter2 button and verify
        await Page.ClickAsync("[data-qa='Counter2'] button");
        await Expect(counter1Locator).ToHaveTextAsync("CounterState.Count: 13");
        await Expect(counter2Locator).ToHaveTextAsync("CounterState.Count: 13");
        
        // Check local storage before reload
        string localStorageBeforeWait = await Page.EvaluateAsync<string>("() => JSON.stringify(window.localStorage)");
        Console.WriteLine($"Local Storage before wait: {localStorageBeforeWait}");
        
        // wait for 10 seconds
        // await Task.Delay(10000);
        // Wait until local storage is not empty
        await WaitForLocalStorageNotEmptyAsync();
        
        // Check local storage before reload
        string localStorageBeforeReload = await Page.EvaluateAsync<string>("() => JSON.stringify(window.localStorage)");
        Console.WriteLine($"Local Storage after wait before reload: {localStorageBeforeReload}");
        
        // Reload the page
        await Page.ReloadAsync();
        
        // Check local storage after reload
        string localStorageAfterReload = await Page.EvaluateAsync<string>("() => JSON.stringify(window.localStorage)");
        Console.WriteLine($"Local Storage after reload: {localStorageAfterReload}");

        // Validate Current Render Mode after reload
        await Expect(currentRenderModeLocator).ToHaveTextAsync("Current Render Mode: Wasm");

        // Validate counter values are reset after reload
        await Expect(counter1Locator).ToHaveTextAsync("CounterState.Count: 3");
        await Expect(counter2Locator).ToHaveTextAsync("CounterState.Count: 3");
    }
    private async Task WaitForLocalStorageNotEmptyAsync()
    {
      const int retries = 10;
      const int delay = 1000;// 1 second

      for (int i = 0; i < retries; i++)
      {
        string localStorageContent = await Page.EvaluateAsync<string>("() => JSON.stringify(window.localStorage)");
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
