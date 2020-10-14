namespace TestApp.Server.Integration.Tests.Infrastructure
{
  using Fixie;
  using Microsoft.AspNetCore.Mvc.Testing;
  using Microsoft.Extensions.DependencyInjection;
  using System;
  using System.Reflection;
  using System.Text.Json;

  [NotTest]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
  public class NotTest : Attribute { }

  [NotTest]
  public class TestingConvention : Discovery, Execution, IDisposable
  {
    const string TestPostfix = "Tests";

    private readonly IServiceScopeFactory ServiceScopeFactory;
    private WebApplicationFactory<Startup> WebApplicationFactory;
    private bool Disposed;

    public TestingConvention()
    {
      Console.WriteLine("==== Constructing TestingConvention ====");
      var testServices = new ServiceCollection();
      ConfigureTestServices(testServices);
      ServiceProvider serviceProvider = testServices.BuildServiceProvider();
      ServiceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();

      Classes.Where(aType => aType.IsPublic && !aType.Has<NotTest>());
      Methods.Where(aMethodInfo => aMethodInfo.Name != nameof(Setup));
    }

    public void Dispose() => Dispose(true);

    public void Execute(TestClass aTestClass)
    {
      Console.WriteLine($"==== Running test on class {aTestClass.Type.FullName}====");
      aTestClass.RunCases
      (
        aCase =>
        {
          using IServiceScope serviceScope = ServiceScopeFactory.CreateScope();
          object instance = serviceScope.ServiceProvider.GetService(aTestClass.Type);
          Setup(instance);

          aCase.Execute(instance);
          instance.Dispose();
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
      WebApplicationFactory = new WebApplicationFactory<Startup>();

      aServiceCollection.AddSingleton(WebApplicationFactory);
      aServiceCollection.AddSingleton(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
      aServiceCollection.Scan
      (
        aTypeSourceSelector => aTypeSourceSelector
          .FromAssemblyOf<TestingConvention>()
          .AddClasses(action: (aClasses) => aClasses.Where(aType => aType.IsPublic && !aType.Has<NotTest>()))
          .AsSelf()
          .WithScopedLifetime()
      );
    }

    private void Dispose(bool aIsDisposing)
    {
      if (!Disposed)
      {
        if (aIsDisposing)
        {
          Console.WriteLine("==== Disposing ====");
          WebApplicationFactory?.Dispose();
          ServiceScopeFactory?.Dispose();
        }
        Disposed = true;
      }
    }
  }
}