namespace CounterPageTests;

[TestClass]
public class CounterTests : PageTest
{
  [TestMethod]
  public async Task TestCounterComponents()
  {
    // Navigate to the Counter page
    string sutBaseUrl = Configuration.GetSutBaseUrl();
    
    // Clear local storage
    await Page.Context.AddInitScriptAsync("window.localStorage.clear();");
    
    // Navigate to the Counter page
    await Page.GotoAsync($"{sutBaseUrl}/counter");
    
    // Validate Current Render Mode
    ILocator currentRenderModeLocator = Page.Locator("[data-qa='current-render-mode']");
    await Expect(currentRenderModeLocator).ToHaveTextAsync("Current Render Mode: Server");

    // Validate Configured Render Mode
    ILocator configuredRenderModeLocator = Page.Locator("[data-qa='configured-render-mode']");
    await Expect(configuredRenderModeLocator).ToHaveTextAsync("Configured Render Mode: Auto");

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
    
    // Now I want to reload the page and verify the count is reset and the current-render-mode is Wasm
    
  }
}
