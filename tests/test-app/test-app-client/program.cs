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

    // Mediator (martinothamar) registers handlers at compile time via its source generator. The
    // assembly markers below must be compile-time `typeof(...)` constants identifying every assembly
    // whose handlers should be registered: this app, TimeWarp.State, and TimeWarp.State.Plus.
    serviceCollection.AddMediator
    (
      options =>
      {
        options.ServiceLifetime = ServiceLifetime.Scoped;
        // Keep the generated Mediator (and its Send overloads) internal so this app's own
        // internal action types don't trip CS0051 (less-accessible-than-public-method).
        options.GenerateTypesAsInternal = true;
        options.Assemblies =
        [
          typeof(Test.App.Client.AssemblyMarker),
          typeof(TimeWarp.State.AssemblyMarker),
          typeof(TimeWarp.State.Plus.AssemblyMarker)
        ];
      }
    );

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
    // Pre/post processors are IPipelineBehavior implementations under Mediator; register them as such,
    // in the desired pipeline order (resolved from DI at runtime in registration order).
    serviceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(PrePipelineNotificationRequestPreProcessor<,>));
    serviceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(PostPipelineNotificationRequestPostProcessor<,>));
    serviceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(PersistentStatePostProcessor<,>));
    serviceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(ActiveActionBehavior<,>));
    serviceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(EventStreamBehavior<,>));
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
