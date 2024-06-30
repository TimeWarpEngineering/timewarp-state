namespace Client.Integration.Tests.Infrastructure;

[UsedImplicitly]
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
    serviceCollection.AddTimeWarpState
    (
      options => options.Assemblies =
        new[] { typeof(Test.App.Client.Program).GetTypeInfo().Assembly }
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
