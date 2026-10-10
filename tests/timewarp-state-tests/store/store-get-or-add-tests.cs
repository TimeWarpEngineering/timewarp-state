#region Purpose
// Proves Store.GetState survives concurrent first access without throwing,
// initializes once, and re-initializes after Reset/RemoveState.
// Reset drops the initialization task and cancels the removed instance.
#endregion

#region Design
// Uses a real Store with a counting TestState, a ThrowingSender and a RecordingPublisher. 32 tasks released together
// by a Barrier force concurrent first access.
#endregion

namespace StoreGetOrAddTests;

public class Should_
{
  public void Parallel_First_Access_To_Same_State_Does_Not_Throw_And_Initializes_Once()
  {
    Harness harness = CreateHarness();
    const int participantCount = 32;
    using Barrier barrier = new(participantCount);
    Task<TestState>[] tasks = new Task<TestState>[participantCount];
    for (int i = 0; i < participantCount; i++)
    {
      tasks[i] = Task.Run
      (
        () =>
        {
          barrier.SignalAndWait();
          return harness.Store.GetState<TestState>();
        }
      );
    }

    Should.NotThrow(() => Task.WaitAll(tasks));

    TestState canonical = tasks[0].Result;
    canonical.InitializedFlag.ShouldBe(1);
    for (int i = 0; i < participantCount; i++)
    {
      tasks[i].Result.ShouldBeSameAs(canonical);
      tasks[i].Result.InitializedFlag.ShouldBe(1);
    }

    harness.InitializeCounter.Value.ShouldBe(1);
    harness.Publisher.Publications.Count.ShouldBe(1);
    StateInitializedNotification notification =
      harness.Publisher.Publications[0].Notification.ShouldBeOfType<StateInitializedNotification>();
    notification.StateType.ShouldBe(typeof(TestState));
  }

  public void Reset_Drops_The_Initialization_Task_And_Cancels_The_Removed_State()
  {
    Harness harness = CreateHarness();
    TestState original = harness.Store.GetState<TestState>();
    Task? firstInitialization = harness.Store.FindInitializationTask(typeof(TestState));
    firstInitialization.ShouldNotBeNull();

    harness.Store.Reset();

    harness.Store.FindInitializationTask(typeof(TestState)).ShouldBeNull();
    original.WasCancelled.ShouldBeTrue();

    TestState next = harness.Store.GetState<TestState>();
    next.ShouldNotBeSameAs(original);
    Task? secondInitialization = harness.Store.FindInitializationTask(typeof(TestState));
    secondInitialization.ShouldNotBeNull();
    secondInitialization.ShouldNotBeSameAs(firstInitialization);
  }

  public void GetState_After_Reset_Creates_New_Instance_And_Initializes_Again()
  {
    Harness harness = CreateHarness();
    TestState original = harness.Store.GetState<TestState>();
    harness.InitializeCounter.Value.ShouldBe(1);

    harness.Store.Reset();
    TestState next = harness.Store.GetState<TestState>();

    next.ShouldNotBeSameAs(original);
    next.Guid.ShouldNotBe(original.Guid);
    harness.InitializeCounter.Value.ShouldBe(2);
    harness.Publisher.Publications.Count.ShouldBe(2);
  }

  public void GetState_After_RemoveState_Creates_New_Instance_And_Initializes_Again()
  {
    Harness harness = CreateHarness();
    TestState original = harness.Store.GetState<TestState>();
    harness.InitializeCounter.Value.ShouldBe(1);

    harness.Store.RemoveState<TestState>();
    TestState next = harness.Store.GetState<TestState>();

    next.ShouldNotBeSameAs(original);
    next.Guid.ShouldNotBe(original.Guid);
    harness.InitializeCounter.Value.ShouldBe(2);
    harness.Publisher.Publications.Count.ShouldBe(2);
  }

  private static Harness CreateHarness()
  {
    InitializeCounter initializeCounter = new();
    RecordingPublisher recordingPublisher = new();
    ServiceCollection serviceCollection = new();
    serviceCollection.AddSingleton(initializeCounter);
    serviceCollection.AddTransient<TestState>();
    serviceCollection.AddSingleton<ISender<ClientPipeline>>(new ThrowingSender());
    ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
    Store store = new
    (
      NullLogger<Store>.Instance,
      serviceProvider,
      new TimeWarpStateOptions(serviceCollection),
      recordingPublisher
    );

    return new Harness
    {
      Store = store,
      Publisher = recordingPublisher,
      InitializeCounter = initializeCounter
    };
  }

  private sealed class Harness
  {
    public required Store Store { get; init; }
    public required RecordingPublisher Publisher { get; init; }
    public required InitializeCounter InitializeCounter { get; init; }
  }

  private sealed class InitializeCounter
  {
    public int Count;
    public int Value => Volatile.Read(ref Count);
  }

  private sealed class TestState : IState
  {
    private readonly InitializeCounter InitializeCounter;
    public ISender<ClientPipeline> Sender { get; set; } = null!;
    public Guid Guid { get; } = Guid.NewGuid();
    public int InitializedFlag { get; private set; }
    public bool WasCancelled { get; private set; }

    public TestState(InitializeCounter initializeCounter)
    {
      InitializeCounter = initializeCounter;
    }

    public void Initialize()
    {
      InitializedFlag = 1;
      Interlocked.Increment(ref InitializeCounter.Count);
    }

    public void CancelOperations() => WasCancelled = true;
  }

  private sealed class ThrowingSender : ISender<ClientPipeline>
  {
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
      throw new NotSupportedException();
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
      throw new NotSupportedException();
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
      throw new NotSupportedException();
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>
    (
      IStreamRequest<TResponse> request,
      CancellationToken cancellationToken = default
    )
    {
      throw new NotSupportedException();
    }

    public IAsyncEnumerable<object?> CreateStream
    (
      object request,
      CancellationToken cancellationToken = default
    )
    {
      throw new NotSupportedException();
    }
  }

  private sealed class RecordingPublisher : IPublisher<ClientPipeline>
  {
    public List<(object Notification, CancellationToken CancellationToken)> Publications { get; } = [];

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      Publications.Add((notification, cancellationToken));
      return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
      where TNotification : INotification
    {
      return Publish((object)notification, cancellationToken);
    }
  }
}
