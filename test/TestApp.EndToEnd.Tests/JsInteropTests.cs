namespace TestApp.EndToEnd.Tests
{
  using TestApp.EndToEnd.Tests.Infrastructure;
  using OpenQA.Selenium;
  using Shouldly;
  using static Infrastructure.WaitAndAssert;

  public class JsInteropTests : BaseTest
  {
    /// <summary>
    ///
    /// </summary>
    /// <param name="aWebDriver"></param>
    /// <param name="aServerFixture">
    /// Is a dependency as the server needs to be running
    /// but is not referenced otherwise thus the injected item is NOT stored
    /// </param>
    public JsInteropTests(IWebDriver aWebDriver, ServerFixture aServerFixture)
      : base(aWebDriver, aServerFixture)
    {
      WebDriver = aWebDriver;
      aServerFixture.Environment = AspNetEnvironment.Development;
      aServerFixture.BuildWebHostMethod = Server.Program.BuildWebHost;

      Navigate("/", aReload: true);
      WaitUntilLoaded();

      JavaScriptExecutor = WebDriver as IJavaScriptExecutor;
      object clientApplication = JavaScriptExecutor.ExecuteScript("return window.localStorage.getItem('clientApplication');");
      clientApplication.ShouldBe("TestApp.Client.0.0.1"); // Confirm we are running the right app
    }

    private IWebDriver WebDriver { get; }
    private IJavaScriptExecutor JavaScriptExecutor { get; }

    private bool IsClientSide { get; set; }

    public void InitalizationWorkedClientSide()
    {
      JavaScriptExecutor.ExecuteScript("window.localStorage.setItem('executionSide','client');");

      Navigate("/", aReload: true);
      WaitUntilLoaded();
      IsClientSide = true;
      InitalizationWorked();
    }

    public void InitalizationWorkedServerSide()
    {

      JavaScriptExecutor.ExecuteScript("window.localStorage.setItem('executionSide','server');");

      Navigate("/", aReload: true);
      WaitUntilLoaded();
      IsClientSide = false;
      InitalizationWorked();
    }

    private void InitalizationWorked()
    {

      object blazorState = JavaScriptExecutor.ExecuteScript("return window.BlazorState;");
      blazorState.ShouldNotBeNull();

      object initializeJavaScriptInterop = JavaScriptExecutor.ExecuteScript("return window.InitializeJavaScriptInterop;");
      initializeJavaScriptInterop.ShouldNotBeNull();

      object reduxDevToolsFactory = JavaScriptExecutor.ExecuteScript("return window.ReduxDevToolsFactory;");
      reduxDevToolsFactory.ShouldNotBeNull();

      if (IsClientSide)
      {
        object reduxDevTools = JavaScriptExecutor.ExecuteScript("return window.reduxDevTools;");
        reduxDevTools.ShouldNotBeNull();
      }

      object jsonRequestHandler = JavaScriptExecutor.ExecuteScript("return window.jsonRequestHandler;");
      jsonRequestHandler.ShouldNotBeNull();
    }

    public void CanCallCsharpFromJs()
    {

    }

    public void CanCallJsFromCsharp()
    {
      // If Initialization of jsonRequestHandler worked then this worked becuase it calls js from CS.
    }
  }
}