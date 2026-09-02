namespace Sample03Wasm;

public class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    builder.Services.AddScoped
    (
      sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }
    );
    
    // AddGeneratedMediator() is emitted by the TimeWarp.Mediator.Generators source generator into
    // this host assembly.
    builder.Services.AddGeneratedMediator();

    builder.Services.AddTimeWarpState
    (
      options =>
      {
        options.UseReduxDevTools();
      }
    );
    
    builder.Services.AddTimeWarpStateRouting();

    await builder.Build().RunAsync();
  }
}
