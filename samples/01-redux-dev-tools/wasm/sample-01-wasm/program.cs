#region Purpose
// Entry point for the Redux DevTools sample.
#endregion

#region Design
// Same setup as the basic WebAssembly sample, plus options.UseReduxDevTools() in AddTimeWarpState so
// actions and state show up in the browser Redux DevTools extension.
#endregion

namespace Sample01Wasm;

public class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    // AddGeneratedMediator<ClientPipeline>() is emitted by the TimeWarp.Mediator.Generators source
    // generator into this host assembly, scoped to the client pipeline (see mediator-scope.cs).
    builder.Services.AddGeneratedMediator<ClientPipeline>();

    builder.Services.AddTimeWarpState
    (
      options =>
      {
        options.UseReduxDevTools(); // Enable Redux DevTools
      }
    );

    await builder.Build().RunAsync();
  }
}
