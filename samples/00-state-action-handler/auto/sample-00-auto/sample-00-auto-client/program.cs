#region Purpose
// WebAssembly entry point for the Auto sample client, and the shared service registration the server host reuses.
#endregion

#region Design
// ConfigureServices is public static so the server Program can call it and both render modes get the same
// services: AddGeneratedMediator<ClientPipeline>(), AddTimeWarpState() and AddTimeWarpStateBlazor().
#endregion

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
        // AddGeneratedMediator<ClientPipeline>() is emitted by the TimeWarp.Mediator.Generators source
        // generator into this host assembly, scoped to the client pipeline (see mediator-scope.cs).
        serviceCollection.AddGeneratedMediator<ClientPipeline>();

        serviceCollection.AddTimeWarpState();
        serviceCollection.AddTimeWarpStateBlazor();
    }
}
