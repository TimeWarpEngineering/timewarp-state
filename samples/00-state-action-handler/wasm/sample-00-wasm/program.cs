namespace Sample00Wasm;

public class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    // AddGeneratedMediator() is emitted by the TimeWarp.Mediator.Generators source generator into
    // this host assembly.
    builder.Services.AddGeneratedMediator();

    builder.Services.AddTimeWarpState();

    await builder.Build().RunAsync();
  }
}
