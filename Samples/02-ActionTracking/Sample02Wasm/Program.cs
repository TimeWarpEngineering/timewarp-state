namespace Sample02Wasm;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        
        builder.Services.AddTimeWarpState();
        builder.Services.AddTimeWarpStatePlus(); // Adds Action Tracking and other Plus features

        await builder.Build().RunAsync();
    }
}
