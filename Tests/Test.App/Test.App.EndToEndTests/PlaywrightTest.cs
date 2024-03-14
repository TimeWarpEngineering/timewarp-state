using Microsoft.Playwright;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Test.App.EndToEndTests;

  [TestFixture]
  public class BlazorEndToEndTest 
  {
      private IPlaywright _playwright;
      private IBrowser _browser;
      private IPage _page;
      public const string LocalHost = "https://localhost:7011";

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
        await _page.GotoAsync($"{LocalHost}/weather");

        await _page.WaitForSelectorAsync(".table");
        System.Collections.Generic.IReadOnlyList<IElementHandle> rows = await _page.QuerySelectorAllAsync(".table tbody tr");

        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(5, rows.Count);
      }

      [Test]
      public async Task CounterComponentsShouldIncrementCount()
      {
         await _page.GotoAsync($"{LocalHost}/counter");

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
          await _page.GotoAsync($"{LocalHost}/JavaScriptInteropPage"); // Replace URL with the actual URL

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
          await _page.GotoAsync($"{LocalHost}/throwexception"); // Replace URL with the actual URL

          // Wait for the GUID element to be rendered
          await _page.WaitForSelectorAsync("[data-qa='ThrowClientException']");

          // Select the element using data-qa attribute
          var initialElement = await _page.QuerySelectorAsync("[data-qa='CounterStateId']");

          // Extract the text content of the element
          var initialCounterStateId = await initialElement.TextContentAsync();

          // Click the button to throw a client-side exception
          await _page.ClickAsync("[data-qa='ThrowClientException']");


          await Task.Delay(1000); // Adjust the delay time as needed

          await _page.ClickAsync("[data-qa='ThrowServerException']");


          // Add a delay to allow time for the GUID to roll back
          await Task.Delay(1000); // Adjust the delay time as needed

          // Select the element using data-qa attribute
          var finalElement = await _page.QuerySelectorAsync("[data-qa='CounterStateId']");

          // Extract the text content of the element
          var finialCounterStateId = await initialElement.TextContentAsync();

          // Verify that the GUID has rolled back to the initial value after the client-side exception
          Assert.AreEqual(initialCounterStateId, finialCounterStateId);

      }
      [Test]
      public async Task TestGoBackButton()
      {
          var initialUrl = $"{LocalHost}";
          // Navigate to the Blazor page
          await _page.GotoAsync(initialUrl);
          // Wait for the navigation link to appear
          await _page.WaitForSelectorAsync(".nav-link");

          // Click on the navigation link for the GoBackPage
          await _page.ClickAsync("text=Go Back Page");

          await _page.WaitForURLAsync($"{LocalHost}/goback");

           // Wait for the page to load
           await _page.WaitForSelectorAsync("[data-qa='GoBack11']");
          // Click the 'Go Back' button
          await _page.ClickAsync("[data-qa='GoBack11']");
          // Get the current URL after navigation
          await _page.WaitForURLAsync($"{LocalHost}/");
          var currentUrl = _page.Url;

          // Check if the current URL is the home page
          Assert.AreEqual(initialUrl, currentUrl);
      }
      [Test]
      public async Task TestResetStoreButton()
      {
          var initialUrl = $"{LocalHost}/";
          // Navigate to the Blazor page
          await _page.GotoAsync(initialUrl);
          // Wait for the navigation link to appear
          await _page.WaitForSelectorAsync(".nav-link");

          // Click on the navigation link for the GoBackPage
          await _page.ClickAsync("text=Reset Store Page");

          await _page.WaitForURLAsync($"{LocalHost}/ResetStorePage");

          await _page.WaitForSelectorAsync("[data-qa='ResetButton1']");
          // Click the 'Go Back' button
          await _page.ClickAsync("[data-qa='ResetButton1']");
          // Get the current URL after navigation
          //await _page.WaitForURLAsync($"{LocalHost}");
          // Get the current URL after navigation
          var currentUrl = _page.Url;
          Console.WriteLine(currentUrl);
          // Check if the current URL is the home page
          Assert.AreEqual(initialUrl, currentUrl);
      }

        [Test]
        public async Task EventStreamShouldAddStartAndCompleteEvents()
        {
            // Navigate to the Event Stream page
            await _page.GotoAsync($"{LocalHost}/eventstream");

            await _page.WaitForSelectorAsync("[data-qa='increaseCounterButton']");
            // Click the button to increment the CounterState.Count
            await _page.ClickAsync("[data-qa='increaseCounterButton'] button");
           // Wait for the new events to be added
            await _page.WaitForSelectorAsync("[data-qa='eventLists'] li");

            // Get the text content of the events list
            var eventsList = await _page.QuerySelectorAllAsync("[data-qa='eventLists'] li");
            var events = await Task.WhenAll(eventsList.Select(async element => await element.TextContentAsync()));

            // Assert that start and complete events for the CounterState+IncrementCount+Action are added
            Assert.IsTrue(events.Any(eventText => eventText.Contains("Start:Test.App.Client.Features.Counter.CounterState+IncrementCount+Action")));
            Assert.IsTrue(events.Any(eventText => eventText.Contains("Completed:Test.App.Client.Features.Counter.CounterState+IncrementCount+Action")));
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
