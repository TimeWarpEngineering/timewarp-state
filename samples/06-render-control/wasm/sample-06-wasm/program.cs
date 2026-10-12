#region Purpose
// Blazor WebAssembly host for the TimeWarpStateComponent render-control sample.
#endregion

#region Design
// CaptureRenderCaller is off by default so ShouldRender, SetParametersAsync, and StateHasChanged
// skip StackTrace. This host turns it on because the page prints ShouldRenderWasCalledBy.
// AddTimeWarpStateBlazor registers render subscriptions.
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
      }
    );
    builder.Services.AddTimeWarpStateBlazor();

    await builder.Build().RunAsync();
  }
}
