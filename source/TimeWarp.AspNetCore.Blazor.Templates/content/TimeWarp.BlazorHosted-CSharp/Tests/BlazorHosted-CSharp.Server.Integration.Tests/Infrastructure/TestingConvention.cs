namespace BlazorHosted_CSharp.Server.Integration.Tests.Infrastructure
{
  using Fixie;
  using Microsoft.Extensions.DependencyInjection;

  public class TestingConvention : Discovery, Execution
  {
    public TestingConvention()
    {
      var testServices = new ServiceCollection();
      ConfigureTestServices(testServices);
      ServiceProvider serviceProvider = testServices.BuildServiceProvider();
      ServiceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
      Methods.Where(aMethodExpression => aMethodExpression.Name != nameof(Setup));
    }

    private IServiceScopeFactory ServiceScopeFactory { get; }
    public void Execute(TestClass aTestClass)
    {
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
    private void ConfigureTestServices(ServiceCollection aServiceCollection)
    {
      aServiceCollection.AddSingleton<BlazorStateTestServer>();
      aServiceCollection.Scan(aTypeSourceSelector => aTypeSourceSelector
        // Start with all non abstract types in this assembly
        .FromAssemblyOf<TestingConvention>()
        .AddClasses(action: (aClasses) => aClasses.TypeName().EndsWith("Tests"))
        .AsSelf()
        .WithScopedLifetime());
    }
  }
}
