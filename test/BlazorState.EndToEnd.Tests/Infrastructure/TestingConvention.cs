namespace BlazorState.EndToEnd.Tests
{
  using System;
  using BlazorState.EndToEnd.Tests.Infrastructure;
  using Fixie;
  using Microsoft.Extensions.DependencyInjection;

  public class TestingConvention : Discovery, Execution, IDisposable
  {
    
    public TestingConvention()
    {
      Methods.Where(aMethodExpression => aMethodExpression.Name != nameof(Setup));
    }

    private IServiceScopeFactory ServiceScopeFactory { get; set; }
    private BrowserFixture BrowserFixture { get; set; }

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
      BrowserFixture = new BrowserFixture();
      aServiceCollection.AddSingleton(BrowserFixture.WebDriver);
      aServiceCollection.AddSingleton(new ServerFixture());
      //TODO use Scrutor to register all Tests classes
      aServiceCollection.AddScoped<CounterPageTests>();
      aServiceCollection.AddScoped<FetchDataPageTests>();
    }

    public void Dispose() => BrowserFixture.WebDriver.Quit();
  }
}