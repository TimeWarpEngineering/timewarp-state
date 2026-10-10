#region Purpose
// Proves StateTransactionBehavior rolls back on handler failure, publishes ExceptionNotification with
// CancellationToken.None for non-cancellation errors, and skips notification for OperationCanceledException.
#endregion

#region Design
// Constructs StateTransactionBehavior directly over a RecordingStore and RecordingPublisher. TransactionTestState
// implements ICloneable so the clone is predictable; next mutates the current state and then fails, so a rollback
// shows up as the original instance with its original value. EqualGuidState.Clone is MemberwiseClone, so Guid is
// copied. EmptyGuidState.Clone returns Guid.Empty. ThrowingConstructorState is not ICloneable and not a State<T>,
// so the registry has no generated clone and Clone throws InvalidOperationException.
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

  public async Task Throw_InvalidCloneException_When_Clone_Copies_Guid()
  {
    Guid originalGuid = Guid.NewGuid();
    EqualGuidState originalState = new(originalGuid);
    StateTransactionBehavior<EqualGuidState.ThrowAction, Unit> behavior = CreateBehavior<EqualGuidState.ThrowAction>(originalState);

    InvalidCloneException exception = await Should.ThrowAsync<InvalidCloneException>
    (
      () => behavior.Handle
      (
        new EqualGuidState.ThrowAction(),
        _ => Task.FromResult(Unit.Value),
        CancellationToken.None
      )
    );

    exception.EnclosingStateType.ShouldBe(typeof(EqualGuidState));
    exception.CloneCause.ShouldBe(InvalidCloneException.Cause.EqualGuid);
    exception.Message.ShouldContain("equal Guid");
  }

  public async Task Throw_InvalidCloneException_When_Clone_Guid_Is_Empty()
  {
    EmptyGuidState originalState = new(Guid.NewGuid());
    StateTransactionBehavior<EmptyGuidState.ThrowAction, Unit> behavior = CreateBehavior<EmptyGuidState.ThrowAction>(originalState);

    InvalidCloneException exception = await Should.ThrowAsync<InvalidCloneException>
    (
      () => behavior.Handle
      (
        new EmptyGuidState.ThrowAction(),
        _ => Task.FromResult(Unit.Value),
        CancellationToken.None
      )
    );

    exception.EnclosingStateType.ShouldBe(typeof(EmptyGuidState));
    exception.CloneCause.ShouldBe(InvalidCloneException.Cause.EmptyGuid);
    exception.Message.ShouldContain("empty Guid");
    exception.Message.ShouldContain("construct the clone so the initializer runs");
  }

  public async Task Throw_When_State_Has_No_Clone()
  {
    ThrowingConstructorState originalState = new(seed: 1);
    originalState.Guid.ShouldNotBe(Guid.Empty);
    StateTransactionBehavior<ThrowingConstructorState.ThrowAction, Unit> behavior =
      CreateBehavior<ThrowingConstructorState.ThrowAction>(originalState);

    InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>
    (
      () => behavior.Handle
      (
        new ThrowingConstructorState.ThrowAction(),
        _ => Task.FromResult(Unit.Value),
        CancellationToken.None
      )
    );

    exception.Message.ShouldContain(nameof(ThrowingConstructorState));
    exception.Message.ShouldContain("ICloneable");
  }

  private static StateTransactionBehavior<TAction, Unit> CreateBehavior<TAction>(IState originalState)
    where TAction : IAction
  {
    RecordingStore recordingStore = new(originalState);
    RecordingPublisher recordingPublisher = new();
    return new StateTransactionBehavior<TAction, Unit>
    (
      NullLogger<StateTransactionBehavior<TAction, Unit>>.Instance,
      recordingStore,
      recordingPublisher,
      new TimeWarpStateOptions(new ServiceCollection())
    );
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
