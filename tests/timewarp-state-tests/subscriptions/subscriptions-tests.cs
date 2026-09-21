namespace SubscriptionsTests;

public class Should_
{
  public static void ReRenderSubscribers_Racing_Remove_Does_Not_Throw()
  {
    Subscriptions subscriptions = CreateSubscriptions();
    const int componentCount = 200;
    List<TestableComponent> components = new(componentCount);
    for (int i = 0; i < componentCount; i++)
    {
      TestableComponent component = new($"race-{i}");
      components.Add(component);
      subscriptions.Add<TestState>(component);
    }

    using Barrier barrier = new(2);
    Task reRenderTask = Task.Run(() =>
    {
      barrier.SignalAndWait();
      for (int i = 0; i < 1000; i++)
      {
        subscriptions.ReRenderSubscribers<TestState>();
      }
    });
    Task removeTask = Task.Run(() =>
    {
      barrier.SignalAndWait();
      foreach (TestableComponent component in components)
      {
        subscriptions.Remove(component);
      }
    });

    Task.WaitAll(reRenderTask, removeTask);
  }

  public static void ReRenderSubscribers_Drops_Dead_WeakReferences()
  {
    CreateDeadSubscription(out Subscriptions subscriptions, out WeakReference weakReference);

    CollectUntilDead(weakReference);
    weakReference.IsAlive.ShouldBeFalse(
      "GC did not collect the subscriber; dead-ref cleanup cannot be proven in this run.");

    subscriptions.ReRenderSubscribers<TestState>();

    TestableComponent replacement = new("dead-1");
    subscriptions.Add<TestState>(replacement);
    subscriptions.ReRenderSubscribers<TestState>();
    replacement.ReRenderCount.ShouldBe(1);
  }

  private static Subscriptions CreateSubscriptions() => new(NullLogger<Subscriptions>.Instance);

  [MethodImpl(MethodImplOptions.NoInlining)]
  private static void CreateDeadSubscription(out Subscriptions subscriptions, out WeakReference weakReference)
  {
    subscriptions = CreateSubscriptions();
    TestableComponent component = new("dead-1");
    subscriptions.Add<TestState>(component);
    weakReference = new WeakReference(component);
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
  private static void CollectUntilDead(WeakReference weakReference)
  {
    for (int i = 0; i < 10; i++)
    {
      GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
      GC.WaitForPendingFinalizers();
      GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
      if (!weakReference.IsAlive)
      {
        return;
      }
    }
  }

  [NotTest]
  private sealed class TestState : IState
  {
    public ISender<ClientPipeline> Sender { get; set; } = null!;
    public Guid Guid { get; } = Guid.Empty;
    public void Initialize() { }
    public void CancelOperations() { }
  }

  [NotTest]
  private sealed class TestableComponent : ITimeWarpStateComponent
  {
    public string Id { get; }
    public int ReRenderCount { get; private set; }
    public bool ShouldReRenderValue { get; set; } = true;

    public TestableComponent(string id) => Id = id;
    public void ReRender() => ReRenderCount++;
    public bool ShouldReRender(Type stateType) => ShouldReRenderValue;
  }
}
