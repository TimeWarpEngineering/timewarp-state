namespace Another.Tests.Infrastructure
{
  using Fixie;
  using Microsoft.AspNetCore.Mvc.Testing;
  using Microsoft.Extensions.DependencyInjection;
  using System.Net.Http;
  using System.Reflection;
  using System.Text.Json;

  public class TestingConvention : Discovery, Execution
  {
    const string TestPostfix = "_Tests";

    private readonly IServiceScopeFactory ServiceScopeFactory;
    private HttpClient ServerHttpClient;
    private WebApplicationFactory<TestApp.Server.Startup> ServerWebApplicationFactory;
    public TestingConvention()
    {
      var testServices = new ServiceCollection();
      ConfigureTestServices(testServices);
      ServiceProvider serviceProvider = testServices.BuildServiceProvider();
      ServiceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();

      Classes.Where(aType => aType.Namespace.EndsWith(TestPostfix));
      Methods.Where(aMethodInfo => aMethodInfo.Name != nameof(Setup));
    }

    public void Execute(TestClass aTestClass)
    {
      aTestClass.RunCases
      (
        aCase =>
        {
          using IServiceScope serviceScope = ServiceScopeFactory.CreateScope();
          object instance = serviceScope.ServiceProvider.GetService(aTestClass.Type);
          Setup(instance);

          aCase.Execute(instance);
        }
      );
    }

    private static void Setup(object aInstance)
    {
      MethodInfo methodInfo = aInstance.GetType().GetMethod(nameof(Setup));
      methodInfo?.Execute(aInstance);
    }

    private void ConfigureTestServices(ServiceCollection aServiceCollection)
    {
      aServiceCollection.AddSingleton(new WebApplicationFactory<TestApp.Server.Startup>());
      aServiceCollection.AddSingleton(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
      aServiceCollection.Scan
      (
        aTypeSourceSelector => aTypeSourceSelector
          .FromAssemblyOf<TestingConvention>()
          .AddClasses(action: (aClasses) => aClasses.Where(aClass => aClass.Namespace.EndsWith(TestPostfix)))
          .AsSelf()
          .WithScopedLifetime()
      );
    }
  }
}
