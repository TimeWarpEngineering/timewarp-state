namespace Sample00.Client;

public class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        ConfigureServices(builder.Services);
        await builder.Build().RunAsync();
    }

    public static void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddTimeWarpState();
    }
}
