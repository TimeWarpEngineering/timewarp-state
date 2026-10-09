#region Purpose
// Entry point for the WebAssembly sample.
#endregion

#region Design
// Adds the App and HeadOutlet root components, then AddGeneratedMediator<ClientPipeline>() and
// AddTimeWarpState() with default options.
#endregion

namespace Sample00Wasm;

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

    builder.Services.AddTimeWarpState();

    await builder.Build().RunAsync();
  }
}
