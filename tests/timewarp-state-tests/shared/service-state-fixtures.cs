#region Purpose
// One ServiceState and TestNavigationManager for every test that needs a State with [CloneShared] injected services.
#endregion

#region Design
// Top-level types in their own namespace so the pipeline and cloning tests share them instead of keeping copies.
// ServiceState is a State<T>, so the source generator emits its clone and the module initializer registers it.
// TickAction exists for the transaction behavior tests, which call the behavior directly and never dispatch it.
#endregion

namespace SharedTestFixtures;

[NotTest]
public sealed class ServiceState : State<ServiceState>
{
  [CloneShared]
  public ILogger<ServiceState> Logger { get; }

  [CloneShared]
  public HttpClient HttpClient { get; }

  [CloneShared]
  public NavigationManager NavigationManager { get; }

  public int Count { get; set; }

  public ServiceState(ILogger<ServiceState>? logger, HttpClient? httpClient, NavigationManager? navigationManager)
  {
    Logger = logger!;
    HttpClient = httpClient!;
    NavigationManager = navigationManager!;
  }

  public override void Initialize() { }

#pragma warning disable TWM001 // Behavior fixture. The tests call the behavior directly and do not dispatch this action.
  public sealed class TickAction : IAction;
#pragma warning restore TWM001
}

[NotTest]
public sealed class TestNavigationManager : NavigationManager
{
  public TestNavigationManager()
  {
    Initialize("https://example.test/", "https://example.test/");
  }

  protected override void NavigateToCore(string uri, bool forceLoad) { }
}
