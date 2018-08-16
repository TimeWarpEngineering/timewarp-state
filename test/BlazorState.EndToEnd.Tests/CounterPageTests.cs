namespace BlazorState.EndToEnd.Tests
{
  using BlazorState.EndToEnd.Tests.Infrastructure;
  using OpenQA.Selenium;
  using Shouldly;

  public class CounterPageTests : BaseTest
  {
    /// <summary>
    ///
    /// </summary>
    /// <param name="aWebDriver"></param>
    /// <param name="aServerFixture">
    /// Is a dependency as the server needs to be running
    /// but is not referenced otherwise thus the injected item is NOT stored
    /// </param>
    public CounterPageTests(IWebDriver aWebDriver, ServerFixture aServerFixture)
      : base(aWebDriver, aServerFixture)
    {
      WebDriver = aWebDriver;
      aServerFixture.Environment = AspNetEnvironment.Development;
      aServerFixture.BuildWebHostMethod = BlazorState.Server.Program.BuildWebHost;

      Navigate("/", noReload: false);
      WaitUntilLoaded();
    }

    private IWebDriver WebDriver { get; }

    public void HasCounterPage()
    {
      // Navigate to "Counter"
      WebDriver.FindElement(By.LinkText("Counter")).Click();
      WaitAssert.Equal("Counter", () => WebDriver.FindElement(By.TagName("h1")).Text);

      // Observe the initial value is 3
      IWebElement countDisplayElement = WebDriver.FindElement(By.CssSelector("h1 + p"));
      countDisplayElement.Text.ShouldBe("Current count: 3");

      // Click the button; see it increment by 5
      IWebElement button = WebDriver.FindElement(By.CssSelector(".main button"));
      button.Click();
      WaitAssert.Equal("Current count: 8", () => countDisplayElement.Text);
      button.Click();
      WaitAssert.Equal("Current count: 13", () => countDisplayElement.Text);
      button.Click();
      WaitAssert.Equal("Current count: 18", () => countDisplayElement.Text);
    }
  }
}