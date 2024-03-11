using Microsoft.Playwright;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Test.App.EndToEndTests;

  [TestFixture]
  public class BlazorEndToEndTest 
  {
      private IPlaywright _playwright;
      private IBrowser _browser;
      private IPage _page;

      [SetUp]
      public async Task SetUp()
      {
          _playwright = await Playwright.CreateAsync();
          _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
          _page = await _browser.NewPageAsync();
      }

      [TearDown]
      public async Task TearDown()
      {
          await _browser.CloseAsync();
          _playwright.Dispose();
      }


      [Test]
      public async Task WeatherForecastTableShouldBeRendered()
      {
        await _page.GotoAsync("https://localhost:7011/weather");

        await _page.WaitForSelectorAsync(".table");
        System.Collections.Generic.IReadOnlyList<IElementHandle> rows = await _page.QuerySelectorAllAsync(".table tbody tr");

        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(5, rows.Count);
      }

      [Test]
      public async Task CounterComponentsShouldIncrementCount()
      {
         await _page.GotoAsync("https://localhost:7011/counter");

        // Wait for the Counter components to be rendered
        await _page.WaitForSelectorAsync("[data-qa='Counter1']");
        await _page.WaitForSelectorAsync("[data-qa='Counter2']");

        // Get the initial CounterState.Count value for both counters
        var initialCount1 = await GetCounterCount(_page, "Counter1");
        var initialCount2 = await GetCounterCount(_page, "Counter2");

        // Click the button in Counter1 component
        await _page.ClickAsync("[data-qa='Counter1'] button");

        // Wait for the count to update
        await Task.Delay(500); // Add delay to ensure the count is updated

        // Get the updated CounterState.Count value for both counters
        var updatedCount1 = await GetCounterCount(_page, "Counter1");
        var updatedCount2 = await GetCounterCount(_page, "Counter2");

        // Verify that CounterState.Count is incremented by 5 on Counter1 component
        Assert.AreEqual(8, updatedCount1);

        // Verify that CounterState.Count remains unchanged on Counter2 component
        Assert.AreEqual(8, updatedCount2);

      }

      [Test]
      public async Task JavaScriptInteropShouldIncrementCount()
      {

          // Navigate to the JavaScript Interop page
          await _page.GotoAsync("https://localhost:7011/JavaScriptInteropPage"); // Replace URL with the actual URL

          // Wait for the button to be rendered
          await _page.WaitForSelectorAsync("[data-qa='JsInterop']");

          // Get the initial value of CounterState.Count
          var initialCount = await GetCounterCount(_page);

          // Click the button to increment the count by 7 via JavaScript interop
          await _page.ClickAsync("[data-qa='JsInterop']");

          // Wait for the count to update
          await Task.Delay(500); // Add delay to ensure the count is updated

          // Get the updated value of CounterState.Count
          var updatedCount = await GetCounterCount(_page);

          // Verify that CounterState.Count is incremented by 7
          Assert.AreEqual(initialCount + 7, updatedCount);

      }
      [Test]
      public async Task ExceptionShouldNotChangeGuid()
      {
          // Navigate to the Exception page
          await _page.GotoAsync("https://localhost:7011/throwexception"); // Replace URL with the actual URL

          // Wait for the GUID element to be rendered
          await _page.WaitForSelectorAsync("#exceptionGuid");

          // Get the initial GUID value
          var initialGuid = await GetGuid(_page);

          // Click the button to throw a client-side exception
          await _page.ClickAsync("[data-qa='ThrowClientException']");


          // Add a delay to allow time for the GUID to roll back
          await Task.Delay(2000); // Adjust the delay time as needed

          // Get the updated GUID value after the rollback
          var updatedGuidAfterClientException = await GetGuid(_page);

          // Verify that the GUID has rolled back to the initial value after the client-side exception
          Assert.AreEqual(initialGuid, updatedGuidAfterClientException);

          // Click the button to throw a server-side exception
          await _page.ClickAsync("[data-qa='ThrowServerException']");


          // Add a delay to allow time for the GUID to roll back
          await Task.Delay(2000); // Adjust the delay time as needed

          // Get the updated GUID value after the rollback
          var updatedGuidAfterServerException = await GetGuid(_page);

          // Verify that the GUID has rolled back to the initial value after the server-side exception
          Assert.AreEqual(initialGuid, updatedGuidAfterServerException);

      }
      [Test]
      public async Task TestGoBackButton()
      {
          // Navigate to the Blazor page
          await _page.GotoAsync("https://localhost:7011/goback");

          // Wait for the page to load
          await Task.Delay(5000); // Adjust the delay time as needed

          // Click the 'Go Back' button
          await _page.ClickAsync("#goback-button");

          await _page.WaitForNavigationAsync();
          // Get the current URL after navigation
          var currentUrl = _page.Url;

          // Check if the current URL is the home page
          Assert.AreEqual("https://localhost:7011/", currentUrl);
      }
      [Test]
      public async Task TestResetStoreButton()
      {
          // Navigate to the Reset Store page
          await _page.GotoAsync("https://localhost:7011/ResetStorePage");

          await _page.WaitForSelectorAsync("#resetstore-button", new PageWaitForSelectorOptions { Timeout = 60000 });
          // Click the Reset Store button
          await _page.ClickAsync("#resetstore-button");
          await _page.WaitForNavigationAsync();
          // Get the current URL after navigation
          var currentUrl = _page.Url;

          // Check if the current URL is the home page
          Assert.AreEqual("https://localhost:7011/", currentUrl);
      }



  private async Task<string> GetGuid(IPage page)
      {
          return await page.EvalOnSelectorAsync<string>("#exceptionGuid", "el => el.innerText.trim()");
      }

      private async Task<int> GetCounterCount(IPage page, string counterTestId)
      {
        return await page.EvalOnSelectorAsync<int>($"[data-qa='{counterTestId}'] p", "el => parseInt(el.innerText.split(':')[1])");
      }
      private async Task<int> GetCounterCount(IPage page)
      {
        return await page.EvalOnSelectorAsync<int>("ul li", "el => parseInt(el.innerText.split(':')[1])");
      }
}
