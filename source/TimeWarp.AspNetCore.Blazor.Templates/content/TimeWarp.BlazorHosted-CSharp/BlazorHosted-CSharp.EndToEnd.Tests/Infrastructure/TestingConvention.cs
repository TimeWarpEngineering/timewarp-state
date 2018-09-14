namespace BlazorHosted_CSharp.EndToEnd.Tests
{
  using System;
  using BlazorHosted_CSharp.EndToEnd.Tests.Infrastructure;
  using Fixie;
  using Microsoft.Extensions.DependencyInjection;

  public class TestingConvention : Discovery, Execution, IDisposable
  {
    public TestingConvention()
    {
      Methods.Where(aMethodExpression => aMethodExpression.Name != nameof(Setup));
    }

    private BrowserFixture BrowserFixture { get; set; }
    private IServiceScopeFactory ServiceScopeFactory { get; set; }
    private SeleniumStandAlone SeleniumStandAlone { get; set; }

    public void Dispose()
    {
      BrowserFixture.WebDriver.Quit();
      SeleniumStandAlone.Dispose();
    }

    public void Execute(TestClass aTestClass)
    {
      if (ServiceScopeFactory == null)
      {
        ConfigureServiceProvider();
      }

      aTestClass.RunCases(aCase =>
      {
        using (IServiceScope serviceScope = ServiceScopeFactory.CreateScope())
        {
          object instance = serviceScope.ServiceProvider.GetService(aTestClass.Type);
          Setup(instance);
          aCase.Execute(instance);
        }
      });
    }

    private static void Setup(object aInstance)
    {
      System.Reflection.MethodInfo method = aInstance.GetType().GetMethod(nameof(Setup));
      method?.Execute(aInstance);
    }

    private void ConfigureServiceProvider()
    {
      var testServices = new ServiceCollection();
      ConfigureTestServices(testServices);
      ServiceProvider serviceProvider = testServices.BuildServiceProvider();
      ServiceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
    }

    private void ConfigureTestServices(ServiceCollection aServiceCollection)
    {
      SeleniumStandAlone = new SeleniumStandAlone();
      BrowserFixture = new BrowserFixture(SeleniumStandAlone);
      aServiceCollection.AddSingleton(BrowserFixture.WebDriver);
      aServiceCollection.AddSingleton<ServerFixture>();
      // TODO: should use the same collection as `Classes` here
      aServiceCollection.Scan(aTypeSourceSelector => aTypeSourceSelector
        // Start with all non abstract types in this assembly
        .FromAssemblyOf<TestingConvention>()
        // Add all the classes that end in Tests
        .AddClasses(action: (aClasses) => aClasses.TypeName().EndsWith("Tests"))
        .AsSelf()
        .WithScopedLifetime());
    }
  }
}