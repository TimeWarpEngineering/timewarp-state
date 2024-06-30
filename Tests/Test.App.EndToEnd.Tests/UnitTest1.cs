namespace Test.App.EndToEnd.Tests;

using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

[TestClass]
public class ExampleTest : PageTest
{
  [TestMethod]
  public async Task HasTitle()
  {
    await Page.GotoAsync("https://playwright.dev");

    // Expect a title "to contain" a substring.
    await Expect(Page).ToHaveTitleAsync(new Regex("Playwright"));
  }

  [TestMethod]
  public async Task GetStartedLink()
  {
    await Page.GotoAsync("https://playwright.dev");

    // Click the get started link.
    await Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }).ClickAsync();

    // Expects page to have a heading with the name of Installation.
    await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" })).ToBeVisibleAsync();
  } 
}
