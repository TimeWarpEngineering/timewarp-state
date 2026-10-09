#region Purpose
// Holds the client IServiceProvider that the integration tests resolve services from.
#endregion

#region Design
// [NotTest] so Fixie skips it. Registered as a singleton by the TestingConvention and injected into BaseTest.
#endregion

namespace TestApp.Client.Integration.Tests.Infrastructure;

[NotTest]
public class ClientHost
(
  IServiceProvider serviceProvider
)
{

  public IServiceProvider ServiceProvider { get; } = serviceProvider;
}
