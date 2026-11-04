#region Purpose
// Stores, publishers, and states the transaction behavior tests drive.
#endregion

#region Design
// Nested in Should_ so each case names the fixture without a public test type. Partial keeps the case file under the line cap.
#endregion

namespace StateTransactionBehaviorTests;

public partial class Should_
{
  private static Harness CreateHarness()
  {
    TransactionTestState originalState = new(Guid.NewGuid(), value: 5);
    RecordingStore recordingStore = new(originalState);
    RecordingPublisher recordingPublisher = new();
    StateTransactionBehavior<TransactionTestState.ThrowAction, Unit> behavior = new
    (
      NullLogger<StateTransactionBehavior<TransactionTestState.ThrowAction, Unit>>.Instance,
      recordingStore,
      recordingPublisher,
      new TimeWarpStateOptions(new ServiceCollection())
    );

    return new Harness
    {
      Behavior = behavior,
      Store = recordingStore,
      Publisher = recordingPublisher,
      OriginalState = originalState
    };
  }

  private sealed class Harness
  {
    public required StateTransactionBehavior<TransactionTestState.ThrowAction, Unit> Behavior { get; init; }
    public required RecordingStore Store { get; init; }
    public required RecordingPublisher Publisher { get; init; }
    public required TransactionTestState OriginalState { get; init; }
  }

  private sealed class TransactionTestState : IState, ICloneable
  {
    public ISender<ClientPipeline> Sender { get; set; } = null!;
    public Guid Guid { get; }
    public int Value { get; set; }

    public TransactionTestState(Guid guid, int value)
    {
      Guid = guid;
      Value = value;
    }

    public void Initialize() { }

    public void CancelOperations() { }

    public object Clone() => new TransactionTestState(Guid.NewGuid(), Value);

    public sealed class ThrowAction : IAction;
  }

  private sealed class EqualGuidState : IState, ICloneable
  {
    public ISender<ClientPipeline> Sender { get; set; } = null!;
    public Guid Guid { get; }

    public EqualGuidState(Guid guid)
    {
      Guid = guid;
    }

    public void Initialize() { }

    public void CancelOperations() { }

    public object Clone() => MemberwiseClone();

    public sealed class ThrowAction : IAction;
  }

  private sealed class EmptyGuidState : IState, ICloneable
  {
    public ISender<ClientPipeline> Sender { get; set; } = null!;
    public Guid Guid { get; }

    public EmptyGuidState(Guid guid)
    {
      Guid = guid;
    }

    public void Initialize() { }

    public void CancelOperations() { }

    public object Clone() => new EmptyGuidState(Guid.Empty);

    public sealed class ThrowAction : IAction;
  }

  private sealed class ThrowingConstructorState : IState
  {
    public ISender<ClientPipeline> Sender { get; set; } = null!;

    [System.Runtime.Serialization.IgnoreDataMember]
    public Guid Guid { get; } = Guid.NewGuid();

    public ThrowingConstructorState(int seed)
    {
      if (seed == 0) throw new ArgumentOutOfRangeException(nameof(seed), seed, "Seed must be non-zero.");
    }

    public void Initialize() { }

    public void CancelOperations() { }

    public sealed class ThrowAction : IAction;
  }

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

#pragma warning disable TWM001 // Behavior fixture. The test calls the behavior directly and does not dispatch this action.
    public sealed class TickAction : IAction;
#pragma warning restore TWM001
  }

  [NotTest]
  private sealed class TestNavigationManager : NavigationManager
  {
    public TestNavigationManager()
    {
      Initialize("https://example.test/", "https://example.test/");
    }

    protected override void NavigateToCore(string uri, bool forceLoad) { }
  }

  private sealed class RecordingStore : IStore
  {
    public Guid Guid { get; } = Guid.NewGuid();
    public IState CurrentState { get; private set; }

    public RecordingStore(IState currentState)
    {
      CurrentState = currentState;
    }

    public TState GetState<TState>() where TState : IState => (TState)CurrentState;

    public TState? GetPreviousState<TState>() where TState : IState => default;

    public IState GetState(Type stateType) => CurrentState;

    public Task WaitForInitializationAsync<TState>() where TState : IState => Task.CompletedTask;

    public Task? FindInitializationTask(Type stateType) => null;

    public void SetState(IState newState)
    {
      CurrentState = newState;
    }

    public void RemoveState<TState>() where TState : IState { }

    public void Reset() { }
  }

  private sealed class RecordingPublisher : IPublisher<ClientPipeline>
  {
    public List<(object Notification, CancellationToken CancellationToken)> Publications { get; } = [];

    public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      Publications.Add((notification, cancellationToken));
      return ValueTask.CompletedTask;
    }

    public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
      where TNotification : INotification
    {
      return Publish((object)notification, cancellationToken);
    }
  }

  private sealed class RecordingLogger<T> : ILogger<T>
  {
    public List<LogEntry> Entries { get; } = [];

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>
    (
      LogLevel logLevel,
      EventId eventId,
      TState state,
      Exception? exception,
      Func<TState, Exception?, string> formatter
    )
    {
      Entries.Add(new LogEntry(logLevel, eventId, formatter(state, exception)));
    }

    private sealed class NullScope : IDisposable
    {
      public static readonly NullScope Instance = new();

      public void Dispose() { }
    }
  }

  private sealed class LogEntry
  {
    public LogEntry(LogLevel level, EventId eventId, string message)
    {
      Level = level;
      EventId = eventId;
      Message = message;
    }

    public LogLevel Level { get; }
    public EventId EventId { get; }
    public string Message { get; }
  }
}
