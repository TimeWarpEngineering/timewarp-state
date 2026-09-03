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
        // AddGeneratedMediator() is emitted by the TimeWarp.Mediator.Generators source generator
        // into this host assembly.
        serviceCollection.AddGeneratedMediator();

        serviceCollection.AddTimeWarpState();
    }
}
