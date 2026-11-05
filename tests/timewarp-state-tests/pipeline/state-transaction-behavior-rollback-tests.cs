#region Purpose
// Proves StateTransactionBehavior commits a successful handler's clone, rolls back on handler failure, publishes
// ExceptionNotification with CancellationToken.None for non-cancellation errors, rethrows OperationCanceledException
// without a notification, and rethrows other handler exceptions only when RethrowHandlerExceptions is set.
#endregion

#region Design
// Constructs StateTransactionBehavior directly over a RecordingStore and RecordingPublisher through CreateHarness.
// TransactionTestState implements ICloneable so the clone is predictable; next mutates the live state and then fails,
// so a rollback shows up as the original instance with its original value. Observations from inside next are not asserted
// there: the behavior catches handler exceptions, so assertions run after Handle returns.
#endregion

namespace StateTransactionBehaviorTests;

public partial class Should_
{
  public async Task Commit_Replaced_State_When_Handler_Succeeds()
  {
    TransactionTestState originalState = CreateOriginalState();
    Harness<TransactionTestState.ThrowAction> harness = CreateHarness(originalState);
    IState? liveDuringHandler = null;

    await harness.Behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      (_, _) =>
      {
        liveDuringHandler = harness.Store.CurrentState;
        ((TransactionTestState)liveDuringHandler).Value = 42;
        return new ValueTask<Unit>(Unit.Value);
      },
      CancellationToken.None
    );

    TransactionTestState committed = harness.Store.CurrentState.ShouldBeOfType<TransactionTestState>();
    committed.ShouldNotBeSameAs(originalState);
    committed.ShouldBeSameAs(liveDuringHandler);
    committed.Value.ShouldBe(42);
    committed.Guid.ShouldNotBe(originalState.Guid);
    originalState.Value.ShouldBe(5);
    harness.Publisher.Publications.ShouldBeEmpty();
    harness.Logger.Entries.ShouldNotContain(entry => entry.EventId == EventIds.StateTransactionBehavior_Restoring);
  }

  public async Task Rollback_And_Publish_Notification_When_Handler_Throws_On_Cancelled_Token()
  {
    TransactionTestState originalState = CreateOriginalState();
    Harness<TransactionTestState.ThrowAction> harness = CreateHarness(originalState);
    using CancellationTokenSource cancellationTokenSource = new();
    cancellationTokenSource.Cancel();

    await harness.Behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      MutateThenThrow(harness.Store, new InvalidOperationException("handler failed")),
      cancellationTokenSource.Token
    );

    harness.Store.CurrentState.ShouldBeSameAs(originalState);
    originalState.Value.ShouldBe(5);
    harness.Publisher.Publications.Count.ShouldBe(1);
    harness.Publisher.Publications[0].CancellationToken.ShouldBe(CancellationToken.None);
    ExceptionNotification exceptionNotification =
      harness.Publisher.Publications[0].Notification.ShouldBeOfType<ExceptionNotification>();
    exceptionNotification.Exception.ShouldBeOfType<InvalidOperationException>();
  }

  public async Task Not_Rethrow_Non_Cancellation_Exception_With_Default_Options()
  {
    TransactionTestState originalState = CreateOriginalState();
    Harness<TransactionTestState.ThrowAction> harness = CreateHarness(originalState);

    Unit response = await harness.Behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      MutateThenThrow(harness.Store, new InvalidOperationException("handler failed")),
      CancellationToken.None
    );

    response.ShouldBe(default);
    harness.Store.CurrentState.ShouldBeSameAs(originalState);
    originalState.Value.ShouldBe(5);
    harness.Publisher.Publications.Count.ShouldBe(1);
    ExceptionNotification notification =
      harness.Publisher.Publications[0].Notification.ShouldBeOfType<ExceptionNotification>();
    notification.Exception.Message.ShouldBe("handler failed");
    harness.Logger.Entries.ShouldContain(entry => entry.EventId == EventIds.StateTransactionBehavior_Restoring);
  }

  public async Task Rollback_Without_Notification_When_Handler_Throws_OperationCanceledException()
  {
    TransactionTestState originalState = CreateOriginalState();
    Harness<TransactionTestState.ThrowAction> harness = CreateHarness(originalState);

    await Should.ThrowAsync<OperationCanceledException>
    (
      async () => await harness.Behavior.Handle
      (
        new TransactionTestState.ThrowAction(),
        MutateThenThrow(harness.Store, new OperationCanceledException()),
        CancellationToken.None
      )
    );

    harness.Store.CurrentState.ShouldBeSameAs(originalState);
    originalState.Value.ShouldBe(5);
    harness.Publisher.Publications.ShouldBeEmpty();
  }

  public async Task Rollback_Without_Notification_When_OperationCanceledException_Uses_Cancelled_Token()
  {
    TransactionTestState originalState = CreateOriginalState();
    Harness<TransactionTestState.ThrowAction> harness = CreateHarness(originalState);
    using CancellationTokenSource cancellationTokenSource = new();
    cancellationTokenSource.Cancel();

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> next = (_, cancellationToken) =>
    {
      ((TransactionTestState)harness.Store.CurrentState).Value = 99;
      return new ValueTask<Unit>(Task.FromException<Unit>(new OperationCanceledException(cancellationToken)));
    };

    await Should.ThrowAsync<OperationCanceledException>
    (
      async () => await harness.Behavior.Handle(new TransactionTestState.ThrowAction(), next, cancellationTokenSource.Token)
    );

    harness.Store.CurrentState.ShouldBeSameAs(originalState);
    originalState.Value.ShouldBe(5);
    harness.Publisher.Publications.ShouldBeEmpty();
  }

  public async Task Rethrow_Handler_Exception_After_Notification_When_Option_Is_Set()
  {
    TransactionTestState originalState = CreateOriginalState();
    Harness<TransactionTestState.ThrowAction> harness = CreateHarness
    (
      originalState,
      options => options.RethrowHandlerExceptions = true
    );

    InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>
    (
      async () => await harness.Behavior.Handle
      (
        new TransactionTestState.ThrowAction(),
        MutateThenThrow(harness.Store, new InvalidOperationException("handler failed")),
        CancellationToken.None
      )
    );

    exception.Message.ShouldBe("handler failed");
    harness.Store.CurrentState.ShouldBeSameAs(originalState);
    originalState.Value.ShouldBe(5);
    harness.Publisher.Publications.Count.ShouldBe(1);
    harness.Publisher.Publications[0].CancellationToken.ShouldBe(CancellationToken.None);
  }

  public async Task Rethrow_OperationCanceledException_Without_Notification_When_Option_Is_Set()
  {
    TransactionTestState originalState = CreateOriginalState();
    Harness<TransactionTestState.ThrowAction> harness = CreateHarness
    (
      originalState,
      options => options.RethrowHandlerExceptions = true
    );

    await Should.ThrowAsync<OperationCanceledException>
    (
      async () => await harness.Behavior.Handle
      (
        new TransactionTestState.ThrowAction(),
        MutateThenThrow(harness.Store, new OperationCanceledException()),
        CancellationToken.None
      )
    );

    // Cancellation is never published, with or without the rethrow option.
    harness.Store.CurrentState.ShouldBeSameAs(originalState);
    originalState.Value.ShouldBe(5);
    harness.Publisher.Publications.Count.ShouldBe(0);
    harness.Logger.Entries.ShouldContain(entry => entry.EventId == EventIds.StateTransactionBehavior_Restoring);
  }
}
