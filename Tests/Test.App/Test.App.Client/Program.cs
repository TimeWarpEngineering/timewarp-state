namespace Test.App.Client;

public class Program
{
  private static async Task Main(string[] args)
  {
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    ConfigureServices(builder.Services);
    
    await builder.Build().RunAsync();
  }
  public static void ConfigureServices(IServiceCollection serviceCollection)
  {
    serviceCollection.AddBlazoredSessionStorage();
    serviceCollection.AddBlazoredLocalStorage();

    serviceCollection.AddBlazorState
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
    serviceCollection.AddTransient(typeof(IRequestPreProcessor<>), typeof(PrePipelineNotificationRequestPreProcessor<>));
    serviceCollection.AddTransient(typeof(IRequestPostProcessor<,>), typeof(PostPipelineNotificationRequestPostProcessor<,>));
    serviceCollection.AddTransient(typeof(IRequestPostProcessor<,>), typeof(PersistentStatePostProcessor<,>));
    serviceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(ActiveActionBehavior<,>));
    serviceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(EventStreamBehavior<,>));
    serviceCollection.AddScoped<IPersistenceService, PersistenceService>();
    serviceCollection.AddSingleton(serviceCollection);
  }
}
