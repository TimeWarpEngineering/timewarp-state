#region Purpose
// States, harness, and factory the transaction behavior tests drive. The recording fakes are in state-transaction-behavior-fakes.cs.
#endregion

#region Design
// Nested in Should_ so each case names the fixture without a public test type. Partial keeps the case file under the line cap.
#endregion

namespace StateTransactionBehaviorTests;

public partial class Should_
{
  private static Harness<TAction> CreateHarness<TAction>
  (
    IState originalState,
    Action<TimeWarpStateOptions>? configure = null,
    RecordingLogger<StateTransactionBehavior<TAction, Unit>>? logger = null
  )
    where TAction : IAction
  {
    RecordingStore recordingStore = new(originalState);
    RecordingPublisher recordingPublisher = new();
    RecordingLogger<StateTransactionBehavior<TAction, Unit>> recordingLogger = logger ?? new();
    TimeWarpStateOptions options = new(new ServiceCollection());
    configure?.Invoke(options);

    return new Harness<TAction>
    {
      Behavior = new StateTransactionBehavior<TAction, Unit>(recordingLogger, recordingStore, recordingPublisher, options),
      Store = recordingStore,
      Publisher = recordingPublisher,
      Logger = recordingLogger
    };
  }

  private static Harness<TransactionTestState.ThrowAction> CreateHarness
  (
    TransactionTestState originalState,
    Action<TimeWarpStateOptions>? configure = null
  )
  {
    return CreateHarness<TransactionTestState.ThrowAction>(originalState, configure);
  }

  private static TransactionTestState CreateOriginalState() => new(Guid.NewGuid(), value: 5);

  // next delegate that mutates the live (cloned) state and then fails with the given exception.
  private static MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> MutateThenThrow
  (
    RecordingStore store,
    Exception exception,
    int value = 99
  )
  {
    return (_, _) =>
    {
      ((TransactionTestState)store.CurrentState).Value = value;
      return new ValueTask<Unit>(Task.FromException<Unit>(exception));
    };
  }

  private sealed class Harness<TAction>
    where TAction : IAction
  {
    public required StateTransactionBehavior<TAction, Unit> Behavior { get; init; }
    public required RecordingStore Store { get; init; }
    public required RecordingPublisher Publisher { get; init; }
    public required RecordingLogger<StateTransactionBehavior<TAction, Unit>> Logger { get; init; }
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
}
