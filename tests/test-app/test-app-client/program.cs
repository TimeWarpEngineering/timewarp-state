#region Purpose
// Test.App.Client entry point and the shared service setup also used by the server host.
#endregion

#region Design
// ConfigureServices is public so Test.App.Server can register the same client services for server rendering. It calls
// AddGeneratedMediator<ClientPipeline>() only, so an unscoped ISender injection fails fast. It turns on Redux DevTools
// and CaptureRenderCaller, the action catalog, JavaScript dispatch for IncrementCount, and a HttpClient for
// localhost:7011. The culture is fixed to en-US with ISO date patterns so tests see stable formatting.
#endregion

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
        options.CaptureRenderCaller = true;
        options.Assemblies =
          new[]
          {
                typeof(Test.App.Client.AssemblyMarker).GetTypeInfo().Assembly,
		            typeof(TimeWarp.State.Plus.AssemblyMarker).GetTypeInfo().Assembly
          };
      }
    );
    serviceCollection.AddActionCatalog(typeof(Test.App.Client.AssemblyMarker).Assembly);
    // JavaScriptInteropPage dispatches this action from Test.App.Client.lib.module.js.
    serviceCollection.AddJavaScriptDispatch(b => b.Allow<Test.App.Client.Features.Counter.CounterState.IncrementCountActionSet.Action>());
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
