namespace Client.Integration.Tests.Infrastructure;

public class TestingConvention() : TimeWarp.Fixie.TestingConvention(ConfigureAdditionalServicesCallback)
{
  private static void ConfigureAdditionalServicesCallback(ServiceCollection serviceCollection)
  {
    var serverWebApplicationFactory = new WebApplicationFactory<Test.App.Server.Program>();
    HttpClient serverHttpClient = serverWebApplicationFactory.CreateClient();
  
    ConfigureWebAssemblyHost(serviceCollection, serverHttpClient);
  
    serviceCollection.AddSingleton(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
  }
  
  private static void ConfigureWebAssemblyHost(IServiceCollection serviceCollection, HttpClient serverHttpClient)
  {
    var clientHostBuilder = ClientHostBuilder.CreateDefault();
    ConfigureServices(clientHostBuilder.Services, serverHttpClient);
  
    ClientHost clientHost = clientHostBuilder.Build();
    serviceCollection.AddSingleton(clientHost);
  }
  
  private static void ConfigureServices(IServiceCollection serviceCollection, HttpClient serverHttpClient)
  {
    // Need an HttpClient to talk to the Server side configured before calling AddTimeWarpState.
    serviceCollection.AddSingleton(serverHttpClient);

    // AddGeneratedMediator() is emitted by the TimeWarp.Mediator.Generators source generator into the
    // Test.App.Client assembly. It weaves in every behavior that assembly declares at compile time via
    // [assembly: MediatorBehavior] (mediator-behaviors.cs): PrePipelineNotificationRequestPreProcessor,
    // PostPipelineNotificationRequestPostProcessor, PersistentStatePostProcessor, ActiveActionBehavior and
    // EventStreamBehavior. This host registers no Blazored storage services, so PersistentStatePostProcessor
    // is intentionally inert here (it logs PersistentStatePostProcessor_StorageNotRegistered and skips the save).
    serviceCollection.AddGeneratedMediator();

    serviceCollection.AddTimeWarpState
    (
      options => options.Assemblies =
        new[]
        {
          typeof(Test.App.Client.Program).GetTypeInfo().Assembly,
          typeof(TimeWarp.State.Plus.AssemblyMarker).GetTypeInfo().Assembly
        }
    );

    serviceCollection.AddSingleton
    (
      new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      }
    );
  }
}
