#region Purpose
// Blazor WebAssembly host for the TimeWarpStateComponent render-control sample.
#endregion

#region Design
// CaptureRenderCaller is off by default so ShouldRender, SetParametersAsync, and StateHasChanged
// skip StackTrace. This host turns it on because the page prints ShouldRenderWasCalledBy.
// UseReduxDevTools satisfies CommitHandler, which the mediator links even when the sample
// does not show the extension. The component that would call InitAsync is absent.
// AddGeneratedMediator is emitted into this assembly. There is no reflection AddMediator call.
#endregion

namespace Sample06Wasm;

public class Program
{
  public static async Task Main(string[] args)
  {
    WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    builder.Services.AddGeneratedMediator<ClientPipeline>();

    builder.Services.AddTimeWarpState
    (
      options =>
      {
        // Diagnostic pages read Class.Method from ShouldRenderWasCalledBy.
        // A production host leaves this false.
        options.CaptureRenderCaller = true;

        // CommitHandler is linked by the generated mediator and its constructor needs the
        // redux services. Development validates the container, so those services must exist.
        // This sample does not render <ReduxDevTools />, so InitAsync never runs and actions
        // are not forwarded to the browser extension.
        options.UseReduxDevTools();
      }
    );

    await builder.Build().RunAsync();
  }
}
