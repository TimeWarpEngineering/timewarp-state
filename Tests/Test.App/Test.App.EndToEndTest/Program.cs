namespace Test.App.EndToEndTest;

using Microsoft.Playwright;

internal class Program
{
  static async Task Main(string[] args)
  {
    using var playwright = await Playwright.CreateAsync();
    var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
    {
      Headless = true // Set to false if you want to see the browser
    });
    var page = await browser.NewPageAsync();

    // Navigate to the Blazor page
    await page.GotoAsync("http://localhost:7011/weather"); // Replace URL with the actual URL

    // Wait for the table to be rendered
    await page.WaitForSelectorAsync(".table");

    // Get the rows of the table
    var rows = await page.QuerySelectorAllAsync(".table tbody tr");

    // Verify that there are 5 rows in the table
    if (rows.Count != 5)
    {
      Console.WriteLine("Test failed: Expected 5 rows in the table, but found " + rows.Count);
    }
    else
    {
      Console.WriteLine("Test passed: 5 rows found in the table");
    }

    // Close the browser
    await browser.CloseAsync();

  }
}
