namespace EventStreamPageTests;

[TestClass]
public class EventStreamTests : PageTest
{
  private string SutBaseUrl = null!;
  private ILocator EventsListLocator = null!;
  private ILocator CounterStateCountLocator = null!;
  private ILocator IncrementCounterStateCountButtonLocator = null!;

  [TestInitialize]
  public async Task Initialize()
  {
    SutBaseUrl = Configuration.GetSutBaseUrl();

    // Navigate to the Event Stream page
    await Page.GotoAsync($"{SutBaseUrl}/EventStreamPage");

    // Define the locators for the events list, counter state, and increment button
    EventsListLocator = Page.Locator("[data-qa='events-list']");
    CounterStateCountLocator = Page.Locator("[data-qa='counter'] [data-qa='count']");
    IncrementCounterStateCountButtonLocator = Page.Locator("[data-qa='counter'] button");
  }

  [TestMethod]
  public async Task TestEventStream()
  {
    // Validate Server Side
    await ValidateEventStream(RenderModes.Server);

    // Reload
    await PageUtilities.WaitTillBlazorWasmIsDownloadedAsync(Page);
    await Page.ReloadAsync();
    
    // Validate in Wasm
    await ValidateEventStream(RenderModes.Wasm);
  }

  private async Task ValidateEventStream(string expectedCurrentMode)
  {
    await PageUtilities.ValidateRenderModesAsync(this, Page, expectedCurrentMode, ConfiguredRenderModes.InteractiveAutoRenderMode);

    string initialCounterStateCount = await CounterStateCountLocator.TextContentAsync() ?? throw new InvalidOperationException("Counter state count is null.");
    
    // Click the increment button and validate events list
    await IncrementCounterStateCountButtonLocator.ClickAsync();
    await ValidateEventsList();

    // Validate the counter state count increment
    int expectedCount = int.Parse(initialCounterStateCount) + 5;
    await Expect(CounterStateCountLocator).ToHaveTextAsync(expectedCount.ToString());
  }

  private async Task ValidateEventsList()
  {
    await Expect(EventsListLocator).ToContainTextAsync("Start:Test.App.Client.Features.Counter.CounterState+IncrementCountActionSet+Action");
    await Expect(EventsListLocator).ToContainTextAsync("Completed:Test.App.Client.Features.Counter.CounterState+IncrementCountActionSet+Action");
  }

}
 
