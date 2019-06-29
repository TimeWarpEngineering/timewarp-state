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
      aServerFixture.Environment = AspNetEnvironment.Development;
      aServerFixture.CreateHostBuilderDelegate = Server.Program.CreateHostBuilder;

      Navigate("/", aReload: true);
      WaitUntilLoaded();

      object clientApplication = JavaScriptExecutor.ExecuteScript("return window.localStorage.getItem('clientApplication');");
      clientApplication.ShouldBe("TestApp.Client.0.0.1"); // Confirm we are running the right app
    }

    public void InitalizationWorkedClientSide()
    {
      JavaScriptExecutor.ExecuteScript("window.localStorage.setItem('executionSide','client');");

      Navigate("/", aReload: true);
      WaitUntilLoaded();
      InitalizationWorked();
    }

    public void InitalizationWorkedServerSide()
    {

      JavaScriptExecutor.ExecuteScript("window.localStorage.setItem('executionSide','server');");

      Navigate("/", aReload: true);
      WaitUntilLoaded();
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

      object reduxDevTools = JavaScriptExecutor.ExecuteScript("return window.reduxDevTools;");
      reduxDevTools.ShouldNotBeNull();

      object jsonRequestHandler = JavaScriptExecutor.ExecuteScript("return window.jsonRequestHandler;");
      jsonRequestHandler.ShouldNotBeNull();
    }

    //public void CanCallCsharpFromJs()
    //{
    //  // Redux Dev tool use this ability.
    //  // TODO set up a handler to be launched from JS 
    //}

    //public void CanCallJsFromCsharp()
    //{
    //  // If Initialization of jsonRequestHandler worked then this worked becuase it calls js from CS.
    //}
  }
}