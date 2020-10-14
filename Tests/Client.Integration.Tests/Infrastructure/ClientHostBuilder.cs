namespace TestApp.Client.Integration.Tests.Infrastructure
{
  using Microsoft.Extensions.DependencyInjection;
  using System;

  [NotTest]
  public class ClientHostBuilder
  {
    /// <summary>
    /// Gets the service collection.
    /// </summary>
    public IServiceCollection Services { get; }

    public ClientHostBuilder() { Services = new ServiceCollection(); }

    public static ClientHostBuilder CreateDefault(string[] args = default)
    {
      args ??= Array.Empty<string>();
      var builder = new ClientHostBuilder();

      return builder;
    }

    public ClientHost Build() => new ClientHost(Services.BuildServiceProvider());
  }
}