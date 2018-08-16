using System;
using BlazorState.EndToEnd.Tests.Infrastructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace BlazorState.EndToEnd.Tests
{
  public abstract class BaseTest
  {
    public BaseTest(IWebDriver aWebDriver, ServerFixture aServerFixture)
    {
      WebDriver = aWebDriver;
      ServerFixture = aServerFixture;
    }

    private ServerFixture ServerFixture { get; }
    private IWebDriver WebDriver { get; }

    protected void Navigate(string relativeUrl, bool noReload = false)
    {
      var absoluteUrl = new Uri(ServerFixture.RootUri, relativeUrl);

      if (noReload)
      {
        string existingUrl = WebDriver.Url;
        if (string.Equals(existingUrl, absoluteUrl.AbsoluteUri, StringComparison.Ordinal))
        {
          return;
        }
      }

      WebDriver.Navigate().GoToUrl("about:blank"); // Causes a reload
      WebDriver.Navigate().GoToUrl(absoluteUrl);
    }

    protected void WaitUntilLoaded()
    {
      new WebDriverWait(WebDriver, TimeSpan.FromSeconds(30)).Until(
          driver => driver.FindElement(By.TagName("app")).Text != "Loading...");
    }
  }
}