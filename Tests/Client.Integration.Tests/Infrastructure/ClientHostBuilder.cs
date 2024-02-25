namespace TestApp.Client.Integration.Tests.Infrastructure;

[NotTest]
public class ClientHostBuilder
{
  public IServiceCollection Services { get; } = new ServiceCollection();

  public static ClientHostBuilder CreateDefault() => new();

  public ClientHost Build() => new(Services.BuildServiceProvider());
}
