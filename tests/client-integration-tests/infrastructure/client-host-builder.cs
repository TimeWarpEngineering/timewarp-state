#region Purpose
// Minimal builder that collects services and builds a ClientHost, standing in for WebAssemblyHostBuilder.
#endregion

#region Design
// [NotTest] so Fixie skips it. It only wraps a ServiceCollection; no WebAssembly runtime is involved.
#endregion

namespace TestApp.Client.Integration.Tests.Infrastructure;

[NotTest]
public class ClientHostBuilder
{
  public IServiceCollection Services { get; } = new ServiceCollection();

  public static ClientHostBuilder CreateDefault() => new();

  public ClientHost Build() => new(Services.BuildServiceProvider());
}
