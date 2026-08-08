namespace Sample00Auto.Client;

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
        serviceCollection.AddMediator
        (
          options =>
          {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.GenerateTypesAsInternal = true;
            options.Assemblies = [typeof(Program), typeof(TimeWarp.State.AssemblyMarker)];
          }
        );

        serviceCollection.AddTimeWarpState();
    }
}
