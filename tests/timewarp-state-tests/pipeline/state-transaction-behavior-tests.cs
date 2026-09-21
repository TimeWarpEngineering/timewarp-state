#region Purpose
// Proves StateTransactionBehavior rolls back on handler failure, publishes ExceptionNotification with
// CancellationToken.None for non-cancellation errors, and skips notification for OperationCanceledException.
#endregion

namespace StateTransactionBehaviorTests;

public class Should_
{
  public async Task Rollback_And_Publish_Notification_When_Handler_Throws_On_Cancelled_Token()
  {
    Harness harness = CreateHarness();
    using CancellationTokenSource cancellationTokenSource = new();
    cancellationTokenSource.Cancel();

    RequestHandlerDelegate<Unit> next = _ =>
    {
      var current = (TransactionTestState)harness.Store.CurrentState;
      current.Value = 99;
      return Task.FromException<Unit>(new InvalidOperationException("handler failed"));
    };

    await harness.Behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      next,
      cancellationTokenSource.Token
    );

    harness.Store.CurrentState.ShouldBeSameAs(harness.OriginalState);
    harness.OriginalState.Value.ShouldBe(5);
    harness.Publisher.Publications.Count.ShouldBe(1);
    harness.Publisher.Publications[0].CancellationToken.ShouldBe(CancellationToken.None);
    ExceptionNotification exceptionNotification =
      harness.Publisher.Publications[0].Notification.ShouldBeOfType<ExceptionNotification>();
    exceptionNotification.Exception.ShouldBeOfType<InvalidOperationException>();
  }

  public async Task Rollback_Without_Notification_When_Handler_Throws_OperationCanceledException()
  {
    Harness harness = CreateHarness();

    RequestHandlerDelegate<Unit> next = _ =>
    {
      var current = (TransactionTestState)harness.Store.CurrentState;
      current.Value = 99;
      return Task.FromException<Unit>(new OperationCanceledException());
    };

    await harness.Behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      next,
      CancellationToken.None
    );

    harness.Store.CurrentState.ShouldBeSameAs(harness.OriginalState);
    harness.OriginalState.Value.ShouldBe(5);
    harness.Publisher.Publications.ShouldBeEmpty();
  }

  public async Task Rollback_Without_Notification_When_OperationCanceledException_Uses_Cancelled_Token()
  {
    Harness harness = CreateHarness();
    using CancellationTokenSource cancellationTokenSource = new();
    cancellationTokenSource.Cancel();

    RequestHandlerDelegate<Unit> next = cancellationToken =>
    {
      var current = (TransactionTestState)harness.Store.CurrentState;
      current.Value = 99;
      return Task.FromException<Unit>(new OperationCanceledException(cancellationToken));
    };

    await harness.Behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      next,
      cancellationTokenSource.Token
    );

    harness.Store.CurrentState.ShouldBeSameAs(harness.OriginalState);
    harness.OriginalState.Value.ShouldBe(5);
    harness.Publisher.Publications.ShouldBeEmpty();
  }

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

  private sealed class RecordingStore : IStore
  {
    public Guid Guid { get; } = Guid.NewGuid();
    public IState CurrentState { get; private set; }
    public ConcurrentDictionary<string, Task> StateInitializationTasks { get; } = new();

    public RecordingStore(IState currentState)
    {
      CurrentState = currentState;
    }

    public TState GetState<TState>() where TState : IState => (TState)CurrentState;

    public TState? GetPreviousState<TState>() where TState : IState => default;

    public object GetState(Type stateType) => CurrentState;

    public SemaphoreSlim? GetSemaphore(Type stateType) => null;

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
