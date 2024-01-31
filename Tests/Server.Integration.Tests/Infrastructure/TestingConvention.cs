namespace TestApp.Server.Integration.Tests.Infrastructure;

[NotTest]
public class TestingConvention : TimeWarp.Fixie.TestingConvention 
{
  public TestingConvention():base(ConfigureTestServices) { }

  private static void ConfigureTestServices(ServiceCollection aServiceCollection)
  {
    aServiceCollection.AddSingleton(new WebApplicationFactory<Startup>());
    aServiceCollection.AddSingleton(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
  }
}
