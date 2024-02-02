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
          new Assembly[]
          {
                typeof(Program).GetTypeInfo().Assembly,
		            //typeof(StateInitializedNotificationHandler).GetTypeInfo().Assembly
          };
      }
    );
    serviceCollection.AddTransient(typeof(IRequestPreProcessor<>), typeof(PrePipelineNotificationRequestPreProcessor<>));
    serviceCollection.AddTransient(typeof(IRequestPostProcessor<,>), typeof(PostPipelineNotificationRequestPostProcessor<,>));
    serviceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(Features.EventStream.EventStreamBehavior<,>));
    serviceCollection.AddSingleton(serviceCollection);
  }
}
