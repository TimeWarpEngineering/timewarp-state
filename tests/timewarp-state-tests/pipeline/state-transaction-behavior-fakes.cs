#region Purpose
// Recording fakes for the transaction behavior tests: store and publisher. The logger is shared/recording-logger.cs.
#endregion

#region Design
// Nested in Should_ (partial) like the other fixtures. The store keeps one current state and exposes it directly so tests can
// compare instances by reference; the publisher records every notification with its token.
#endregion

namespace StateTransactionBehaviorTests;

public partial class Should_
{
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
}
