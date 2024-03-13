namespace TestApp.Client.Integration.Tests.Infrastructure;

[NotTest]
public class ClientHost
(
  IServiceProvider serviceProvider
)
{

  public IServiceProvider ServiceProvider { get; } = serviceProvider;
}
