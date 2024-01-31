namespace TestApp.Client.Integration.Tests.Infrastructure;

[NotTest]
public class ClientHost
{

  public ClientHost(ServiceProvider aServiceProvider)
  {
    ServiceProvider = aServiceProvider;
  }

  public IServiceProvider ServiceProvider { get; }
}
