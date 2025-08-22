namespace PersistenceTestPageTests;

[TestClass]
public class PersistenceTest : PageTest
{
  private string SutBaseUrl = null!;
  private ILocator PurpleStateGuidLocator = null!;
  private ILocator PurpleStateCountLocator = null!;
  private ILocator IncrementPurpleCountButtonLocator = null!;
  private ILocator BlueStateGuidLocator = null!;
  private ILocator BlueStateCountLocator = null!;
  private ILocator IncrementBlueCountButtonLocator = null!;
  
  private int ExpectedPurpleCount = 1;
  private int ExpectedBlueCount = 2;
  private const  int PurpleIncrement = 5;
  private const  int BlueIncrement = 3;
    

  [TestInitialize]
  public async Task Initialize()
  {
    SutBaseUrl = Configuration.GetSutBaseUrl() ?? throw new InvalidOperationException("SUT base URL is not configured.");

    // Navigate to the Persistence Test page
    await Page.GotoAsync($"{SutBaseUrl}/PersistenceTestPage");

    // Define the locators for the purple state, blue state, and increment buttons
    PurpleStateGuidLocator = Page.Locator("[data-qa='purple-state-guid']");
    PurpleStateCountLocator = Page.Locator("[data-qa='purple-state-count']");
    IncrementPurpleCountButtonLocator = Page.Locator("[data-qa='increment-purple-count']");
    BlueStateGuidLocator = Page.Locator("[data-qa='blue-state-guid']");
    BlueStateCountLocator = Page.Locator("[data-qa='blue-state-count']");
    IncrementBlueCountButtonLocator = Page.Locator("[data-qa='increment-blue-count']");
  }

  [TestMethod]
  public async Task TestPersistence()
  {
    // Validate Server Side
    await ValidatePersistence(RenderModes.Server);

    // Reload
    await PageUtilities.WaitTillBlazorWasmIsDownloadedAsync(Page);
    await Page.ReloadAsync();
    
    // Validate in Wasm
    await ValidatePersistence(RenderModes.Wasm);
  }

  private async Task ValidatePersistence(string expectedCurrentMode)
  {
    await PageUtilities.ValidateRenderModesAsync(this, Page, expectedCurrentMode, ConfiguredRenderModes.InteractiveAutoRenderMode);
    
    // Click the increment buttons
    await IncrementPurpleCountButtonLocator.ClickAsync();
    ExpectedPurpleCount += PurpleIncrement;
    await IncrementBlueCountButtonLocator.ClickAsync();
    ExpectedBlueCount += BlueIncrement;
    
    await ValidateCount(PurpleStateCountLocator, ExpectedPurpleCount);
    await ValidateCount(BlueStateCountLocator, ExpectedBlueCount);
    
    string initialPurpleStateGuid = await PurpleStateGuidLocator.TextContentAsync() ?? throw new InvalidOperationException("Purple state GUID is null.");
    string initialPurpleStateCount = await PurpleStateCountLocator.TextContentAsync() ?? throw new InvalidOperationException("Purple state count is null.");
    string initialBlueStateGuid = await BlueStateGuidLocator.TextContentAsync() ?? throw new InvalidOperationException("Blue state GUID is null.");
    string initialBlueStateCount = await BlueStateCountLocator.TextContentAsync() ?? throw new InvalidOperationException("Blue state count is null.");

    // Reload the page
    await PageUtilities.WaitTillBlazorWasmIsDownloadedAsync(Page);
    await Page.ReloadAsync();

    // Validate states after reload
    await ValidateState(PurpleStateGuidLocator, initialPurpleStateGuid);
    await ValidateCount(PurpleStateCountLocator, int.Parse(initialPurpleStateCount));
    await ValidateState(BlueStateGuidLocator, initialBlueStateGuid);
    await ValidateCount(BlueStateCountLocator, int.Parse(initialBlueStateCount));

    // Open a new tab and validate
    IPage newPage = await Context.NewPageAsync();
    await newPage.GotoAsync($"{SutBaseUrl}/PersistenceTestPage");

    await ValidateState(newPage.Locator("[data-qa='purple-state-guid']"), initialPurpleStateGuid);
    await ValidateCount(newPage.Locator("[data-qa='purple-state-count']"), int.Parse(initialPurpleStateCount));
    await ValidateState(newPage.Locator("[data-qa='blue-state-guid']"), initialBlueStateGuid, false);
    await ValidateCount(newPage.Locator("[data-qa='blue-state-count']"), 2);
  }

  private async Task ValidateState(ILocator locator, string expectedGuid, bool expectSame = true)
  {
    if (expectSame)
    {
      await Expect(locator).ToHaveTextAsync(expectedGuid, new LocatorAssertionsToHaveTextOptions
        { Timeout = 5000 });
    }
    else
    {
      await Expect(locator).Not.ToHaveTextAsync(expectedGuid, new LocatorAssertionsToHaveTextOptions
        { Timeout = 5000 });
    }
  }

  private async Task ValidateCount(ILocator locator, int expectedCount)
  {
    await Expect(locator).ToHaveTextAsync(expectedCount.ToString(), new LocatorAssertionsToHaveTextOptions
      { Timeout = 5000 });
  }
}
