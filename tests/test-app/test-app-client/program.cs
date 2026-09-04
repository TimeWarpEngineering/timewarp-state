namespace Test.App.Client;

public class Program
{
  private static async Task Main(string[] args)
  {
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    SetIsoCulture();
    ConfigureServices(builder.Services, builder.Configuration);

    WebAssemblyHost webAssemblyHost = builder.Build();
    ILogger<Program> logger = webAssemblyHost.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
    logger.LogInformation("Starting up Client...");
    builder.Services.LogTimeWarpStateMiddleware(logger);

    await webAssemblyHost.RunAsync();
  }
  public static void ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
  {
    serviceCollection.AddLogging();
    serviceCollection.AddBlazoredSessionStorage();
    serviceCollection.AddBlazoredLocalStorage();

    // AddGeneratedMediator<ClientPipeline>() is emitted by the TimeWarp.Mediator.Generators source
    // generator into this host assembly. It registers ISender<ClientPipeline>/IPublisher<ClientPipeline>
    // plus the client-scoped handlers of this app, TimeWarp.State and TimeWarp.State.Plus. Pipeline
    // behaviors are declared at compile time via [assembly: MediatorBehavior] (see mediator-behaviors.cs).
    // The unscoped AddGeneratedMediator() is intentionally not called so an accidental ISender
    // injection fails fast.
    serviceCollection.AddGeneratedMediator<ClientPipeline>();

    serviceCollection.AddTimeWarpState
    (
      options =>
      {
        options
        .UseReduxDevTools
        (
          reduxDevToolsOptions =>
            {
              reduxDevToolsOptions.Name = "Test App";
              reduxDevToolsOptions.Trace = true;
            }
        );
        options.Assemblies =
          new[]
          {
                typeof(Test.App.Client.AssemblyMarker).GetTypeInfo().Assembly,
		            typeof(TimeWarp.State.Plus.AssemblyMarker).GetTypeInfo().Assembly
          };
      }
    );
    serviceCollection.AddScoped<IPersistenceService, PersistenceService>();
    serviceCollection.AddSingleton(serviceCollection);
    serviceCollection.AddTimeWarpStateRouting();

    bool useHttp = configuration.GetValue<bool>("UseHttp");
    string protocol = useHttp ? "http" : "https";
    string baseUrl = $"{protocol}://localhost:7011";

    serviceCollection.AddScoped(sp =>
      new HttpClient
      {
        BaseAddress = new Uri(baseUrl)
      });
  }

  private static void SetIsoCulture()
  {
    var isoCulture =
      new CultureInfo("en-US")
      {
        DateTimeFormat =
        {
          ShortDatePattern = "yyyy-MM-dd", LongDatePattern = "yyyy-MM-ddTHH:mm:ss"
        }
      };

    CultureInfo.DefaultThreadCurrentCulture = isoCulture;
    CultureInfo.DefaultThreadCurrentUICulture = isoCulture;
  }
}
