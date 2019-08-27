namespace TestApp.EndToEnd.Tests
{
  using System;
  using OpenQA.Selenium;
  using OpenQA.Selenium.Support.UI;
  using TestApp.EndToEnd.Tests.Infrastructure;

  public abstract class BaseTest
  {
    protected IJavaScriptExecutor JavaScriptExecutor { get; }
    protected TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    protected IWebDriver WebDriver { get; }

    private ServerFixture ServerFixture { get; }

    public BaseTest(IWebDriver aWebDriver, ServerFixture aServerFixture)
    {
      WebDriver = aWebDriver;
      ServerFixture = aServerFixture;
      JavaScriptExecutor = WebDriver as IJavaScriptExecutor;
    }

    protected void Navigate(string aRelativeUrl, bool aReload = true)
    {
      var absoluteUrl = new Uri(ServerFixture.RootUri, aRelativeUrl);

      if (!aReload && string.Equals(WebDriver.Url, absoluteUrl.AbsoluteUri, StringComparison.Ordinal))
        return;

      WebDriver.Navigate().GoToUrl("about:blank");
      WebDriver.Navigate().GoToUrl(absoluteUrl);
    }

    protected void WaitUntilLoaded()
    {
      new WebDriverWait(WebDriver, Timeout)
        .Until(aWebDriver =>
          JavaScriptExecutor.ExecuteScript("return window.jsonRequestHandler;") != null
          );
    }
  }
}