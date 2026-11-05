#region Purpose
// Proves the generated clone of a [CloneShared] service state keeps the injected services across StateTransactionBehavior actions.
#endregion

#region Design
// Keep_Injected_Services_Across_Two_Actions uses the generated clone of ServiceState. Two successful actions must leave the
// same non-null logger, HttpClient, and NavigationManager on the live state. next only captures what it sees into locals;
// every assertion runs after Handle, because the behavior would swallow an assertion failure thrown inside next.
// The service logger is a RecordingLogger so the log call made inside next is observable.
#endregion

namespace StateTransactionBehaviorTests;

public partial class Should_
{
  public async Task Keep_Injected_Services_Across_Two_Actions()
  {
    RecordingLogger<ServiceState> logger = new();
    using HttpClient httpClient = new();
    NavigationManager navigationManager = new TestNavigationManager();
    ServiceState original = new(logger, httpClient, navigationManager);
    Harness<ServiceState.TickAction> harness = CreateHarness<ServiceState.TickAction>(original);

    List<ServiceState> seenByHandler = [];
    string? baseUriSeenByHandler = null;
    HttpClient? httpClientSeenByHandler = null;

    MessageHandlerDelegate<ServiceState.TickAction, Unit> next = (_, _) =>
    {
      ServiceState current = (ServiceState)harness.Store.GetState(typeof(ServiceState));
      seenByHandler.Add(current);
      current.Logger.LogInformation("tick {Count}", current.Count);
      baseUriSeenByHandler = current.NavigationManager.BaseUri;
      httpClientSeenByHandler = current.HttpClient;
      current.Count++;
      return new ValueTask<Unit>(Unit.Value);
    };

    await harness.Behavior.Handle(new ServiceState.TickAction(), next, CancellationToken.None);
    ServiceState afterFirst = (ServiceState)harness.Store.CurrentState;
    afterFirst.ShouldNotBeSameAs(original);
    afterFirst.Count.ShouldBe(1);
    afterFirst.Logger.ShouldBeSameAs(logger);
    afterFirst.HttpClient.ShouldBeSameAs(httpClient);
    afterFirst.NavigationManager.ShouldBeSameAs(navigationManager);
    seenByHandler.Count.ShouldBe(1);
    seenByHandler[0].ShouldBeSameAs(afterFirst);

    await harness.Behavior.Handle(new ServiceState.TickAction(), next, CancellationToken.None);
    ServiceState afterSecond = (ServiceState)harness.Store.CurrentState;
    afterSecond.ShouldNotBeSameAs(afterFirst);
    afterSecond.ShouldNotBeSameAs(original);
    afterSecond.Count.ShouldBe(2);
    afterSecond.Logger.ShouldBeSameAs(logger);
    afterSecond.HttpClient.ShouldBeSameAs(httpClient);
    afterSecond.NavigationManager.ShouldBeSameAs(navigationManager);
    afterSecond.Guid.ShouldNotBe(original.Guid);
    afterSecond.Guid.ShouldNotBe(afterFirst.Guid);
    seenByHandler.Count.ShouldBe(2);
    seenByHandler[1].ShouldBeSameAs(afterSecond);
    baseUriSeenByHandler.ShouldBe("https://example.test/");
    httpClientSeenByHandler.ShouldBeSameAs(httpClient);
    original.Count.ShouldBe(0);
    harness.Publisher.Publications.ShouldBeEmpty();

    // Both ticks logged through the shared service logger, with the count each handler saw before it incremented.
    logger.Entries.Count.ShouldBe(2);
    logger.Entries[0].Level.ShouldBe(LogLevel.Information);
    logger.Entries[0].Message.ShouldBe("tick 0");
    logger.Entries[1].Message.ShouldBe("tick 1");
  }
}
