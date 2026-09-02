namespace Sample02Wasm;

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
    // this host assembly. Pipeline behaviors are declared at compile time via
    // [assembly: MediatorBehavior] (see mediator-behaviors.cs).
    builder.Services.AddGeneratedMediator();

    builder.Services.AddTimeWarpState
    (
      options =>
      {
        options.Assemblies = new[]
        {
          typeof(Program).Assembly,
          typeof(TimeWarp.State.Plus.AssemblyMarker).Assembly
        };
      }
    );

    await builder.Build().RunAsync();
  }
}
