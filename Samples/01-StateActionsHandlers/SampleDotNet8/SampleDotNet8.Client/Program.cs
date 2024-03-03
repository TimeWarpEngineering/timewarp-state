namespace SampleDotNet8.Client;

using Blazored.LocalStorage;
using TimeWarp.State.Plus.PersistentState;

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
        options.UseReduxDevTools();
        options.Assemblies =
        new[]
        {
          typeof(Program).GetTypeInfo().Assembly,
          typeof(StateInitializedNotificationHandler).GetTypeInfo().Assembly
        };
      }
    );

    serviceCollection.AddScoped<IPersistenceService, PersistenceService>();
    serviceCollection.AddTransient(typeof(IRequestPostProcessor<,>), typeof(PersistentStatePostProcessor<,>));
  }
}
