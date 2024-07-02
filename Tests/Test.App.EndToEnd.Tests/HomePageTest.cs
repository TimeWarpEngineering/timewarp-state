namespace HomePage_;

using FluentAssertions;
using Test.App.EndToEnd.Tests;

[TestClass]
public class Should_ : PageTest
{
  [TestMethod]
  public async Task HaveTitle()
  {
    string sutBaseUrl = Configuration.GetSutBaseUrl();
    Console.WriteLine($"Sut Base Url: {sutBaseUrl}");
    await Page.GotoAsync(sutBaseUrl);
    
    await Expect(Page).ToHaveTitleAsync("TimeWarp.State Test App");
   
    // Validate Current Render Mode
    string? currentRenderModeText = await Page.Locator("text=Current Render Mode: Static").TextContentAsync();
    currentRenderModeText.Should().Be("Current Render Mode: Static");

    // Validate Configured Render Mode
    string? configuredRenderModeText = await Page.Locator("text=Configured Render Mode: None").TextContentAsync();
    configuredRenderModeText.Should().Be("Configured Render Mode: None");
  }
}
