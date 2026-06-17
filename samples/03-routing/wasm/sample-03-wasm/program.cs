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
    
    builder.Services.AddMediator
    (
      options =>
      {
        options.ServiceLifetime = ServiceLifetime.Scoped;
        options.GenerateTypesAsInternal = true;
        options.Assemblies = [typeof(Program), typeof(TimeWarp.State.AssemblyMarker), typeof(TimeWarp.State.Plus.AssemblyMarker)];
      }
    );

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
