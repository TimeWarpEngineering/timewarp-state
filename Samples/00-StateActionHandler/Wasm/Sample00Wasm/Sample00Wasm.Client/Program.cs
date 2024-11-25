namespace Sample00Wasm.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        
        builder.Services.AddTimeWarpState();
        
        await builder.Build().RunAsync();
    }
}
