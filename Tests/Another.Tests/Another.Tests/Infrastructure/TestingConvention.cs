namespace Another.Tests.Infrastructure
{
  using Fixie;
  using Microsoft.Extensions.DependencyInjection;
  public class TestingConvention : Discovery, Execution
  {
    const string TestPostfix = "_Tests";

    private readonly IServiceScopeFactory ServiceScopeFactory;
    public TestingConvention()
    {
      var testServices = new ServiceCollection();
      ConfigureTestServices(testServices);
      ServiceProvider serviceProvider = testServices.BuildServiceProvider();
      ServiceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();

      Classes.Where(aType => aType.Namespace.EndsWith(TestPostfix));
      Methods.Where(aMethodExpression => aMethodExpression.Name != nameof(Setup));
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
      System.Reflection.MethodInfo method = aInstance.GetType().GetMethod(nameof(Setup));
      method?.Execute(aInstance);
    }

    private void ConfigureTestServices(ServiceCollection aServiceCollection)
    {
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
