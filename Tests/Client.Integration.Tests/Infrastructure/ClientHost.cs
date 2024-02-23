namespace TestApp.Client.Integration.Tests.Infrastructure;

[NotTest]
public class ClientHost
(
  IServiceProvider aServiceProvider
)
{

  public IServiceProvider ServiceProvider { get; } = aServiceProvider;
}
