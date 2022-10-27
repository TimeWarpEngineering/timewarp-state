namespace TestApp.Server.Integration.Tests.Infrastructure;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

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
