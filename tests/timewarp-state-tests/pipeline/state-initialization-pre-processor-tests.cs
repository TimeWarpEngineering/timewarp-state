#region Purpose
// StateInitializationPreProcessor waits for an existing initialization task and does not create the state.
#endregion

#region Design
// A lookup store records FindInitializationTask and throws from GetState, so a create would fail the test.
#endregion

namespace StateInitializationPreProcessor_;

public class Should_
{
  public async Task Pass_Through_When_No_Initialization_Task_Is_Registered()
  {
    LookupStore store = new();
    StateInitializationPreProcessor<ProbeState.TickAction, int> processor = Create(store);
    int nextCalls = 0;

    int result = await processor.Handle
    (
      new ProbeState.TickAction(),
      _ =>
      {
        nextCalls++;
        return Task.FromResult(7);
      },
      CancellationToken.None
    );

    result.ShouldBe(7);
    nextCalls.ShouldBe(1);
    store.FindCalls.ShouldBe(1);
  }

  public async Task Wait_For_The_Initialization_Task_Before_Next()
  {
    TaskCompletionSource initialization = new(TaskCreationOptions.RunContinuationsAsynchronously);
    LookupStore store = new() { InitializationTask = initialization.Task };
    StateInitializationPreProcessor<ProbeState.TickAction, int> processor = Create(store);
    int nextCalls = 0;

    Task<int> pending = processor.Handle
    (
      new ProbeState.TickAction(),
      _ =>
      {
        nextCalls++;
        return Task.FromResult(3);
      },
      CancellationToken.None
    );

    nextCalls.ShouldBe(0);
    pending.IsCompleted.ShouldBeFalse();

    initialization.SetResult();
    int result = await pending;

    result.ShouldBe(3);
    nextCalls.ShouldBe(1);
  }

  public async Task Rethrow_When_Initialization_Fails()
  {
    TaskCompletionSource initialization = new();
    initialization.SetException(new InvalidOperationException("load failed"));
    LookupStore store = new() { InitializationTask = initialization.Task };
    StateInitializationPreProcessor<ProbeState.TickAction, int> processor = Create(store);
    int nextCalls = 0;

    InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>
    (
      () => processor.Handle
      (
        new ProbeState.TickAction(),
        _ =>
        {
          nextCalls++;
          return Task.FromResult(0);
        },
        CancellationToken.None
      )
    );

    exception.Message.ShouldBe("load failed");
    nextCalls.ShouldBe(0);
  }

  private static StateInitializationPreProcessor<ProbeState.TickAction, int> Create(LookupStore store) =>
    new(store, NullLogger<StateInitializationPreProcessor<ProbeState.TickAction, int>>.Instance);

  private sealed class ProbeState : State<ProbeState>, ICloneable
  {
    public override void Initialize() { }

    public object Clone() => new ProbeState();

    public sealed class TickAction : IAction;
  }

  private sealed class LookupStore : IStore
  {
    public Task? InitializationTask { get; init; }

    public int FindCalls { get; private set; }

    public Guid Guid => Guid.Empty;

    public Task? FindInitializationTask(Type stateType)
    {
      FindCalls++;
      stateType.ShouldBe(typeof(ProbeState));
      return InitializationTask;
    }

    public TState GetState<TState>() where TState : IState => throw new NotSupportedException();

    public TState? GetPreviousState<TState>() where TState : IState => throw new NotSupportedException();

    public IState GetState(Type stateType) => throw new NotSupportedException();

    public void SetState(IState newState) => throw new NotSupportedException();

    public void RemoveState<TState>() where TState : IState => throw new NotSupportedException();

    public void Reset() => throw new NotSupportedException();

    public Task WaitForInitializationAsync<TState>() where TState : IState => throw new NotSupportedException();
  }
}
