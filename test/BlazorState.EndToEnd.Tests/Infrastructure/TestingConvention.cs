namespace BlazorState.EndToEnd.Tests
{
  using BlazorState.EndToEnd.Tests.Infrastructure;
  using Fixie;
  using Microsoft.Extensions.DependencyInjection;

  public class TestingConvention : Discovery, Execution
  {
    public TestingConvention()
    {
      Methods.Where(aMethodExpression => aMethodExpression.Name != nameof(Setup));
    }

    private IServiceScopeFactory ServiceScopeFactory { get; set; }

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
      var browserFixture = new BrowserFixture();
      aServiceCollection.AddSingleton(browserFixture.WebDriver);
      aServiceCollection.AddSingleton(new ServerFixture());
      aServiceCollection.AddScoped<CounterPageTests>();
    }
  }
}