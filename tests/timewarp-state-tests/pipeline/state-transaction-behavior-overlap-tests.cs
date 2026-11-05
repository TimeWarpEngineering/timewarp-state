#region Purpose
// Proves StateTransactionBehavior leaves a newer clone in place when an overlapping action fails, and restores the
// earlier clone when no newer clone has been committed.
#endregion

#region Design
// Overlap tests share one behavior instance and interleave two next delegates with TaskCompletionSource. The store
// reference, not a timer, decides whether a failure restores its snapshot.
// Every TaskCompletionSource uses RunContinuationsAsynchronously, so SetResult never runs the waiting handler inline on the
// caller's stack. Each ordering is therefore carried by an explicit signal: a handler writes its shared fields before it signals,
// and the waiter reads them after the await. Every await on a signal or a Handle task goes through WithTimeout, so a broken
// interleaving fails with TimeoutException instead of hanging.
#endregion

namespace StateTransactionBehaviorTests;

public partial class Should_
{
  private static TaskCompletionSource<Unit> CreateSignal()
  {
    return new TaskCompletionSource<Unit>(TaskCreationOptions.RunContinuationsAsynchronously);
  }

  public async Task Keep_Later_Commit_When_Earlier_Action_Fails_After_Later_Action_Commits()
  {
    TransactionTestState originalState = CreateOriginalState();
    Harness<TransactionTestState.ThrowAction> harness = CreateHarness(originalState);

    TaskCompletionSource<Unit> laterCommitted = CreateSignal();
    TransactionTestState laterState = null!;

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> earlierNext = async (_, _) =>
    {
      await laterCommitted.Task.WithTimeout();
      throw new InvalidOperationException("earlier action failed");
    };

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> laterNext = (_, _) =>
    {
      laterState = (TransactionTestState)harness.Store.CurrentState;
      laterState.Value = 42;
      laterCommitted.SetResult(Unit.Value);
      return new ValueTask<Unit>(Unit.Value);
    };

    Task earlier = harness.Behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      earlierNext,
      CancellationToken.None
    ).AsTask();
    Task later = harness.Behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      laterNext,
      CancellationToken.None
    ).AsTask();
    await Task.WhenAll(earlier, later).WithTimeout();

    harness.Store.CurrentState.ShouldBeSameAs(laterState);
    laterState.Value.ShouldBe(42);
    originalState.Value.ShouldBe(5);
    harness.Store.CurrentState.ShouldNotBeSameAs(originalState);
    harness.Publisher.Publications.Count.ShouldBe(1);
    harness.Publisher.Publications[0].CancellationToken.ShouldBe(CancellationToken.None);
    harness.Logger.Entries.ShouldContain
    (
      entry => entry.EventId == EventIds.StateTransactionBehavior_ConcurrentAdvance
    );
    harness.Logger.Entries.ShouldNotContain
    (
      entry => entry.EventId == EventIds.StateTransactionBehavior_Restoring
    );
  }

  public async Task Keep_Successor_Commit_When_Overlapping_Action_Fails_While_Earlier_Action_Is_In_Flight()
  {
    TransactionTestState originalState = CreateOriginalState();
    Harness<TransactionTestState.ThrowAction> harness = CreateHarness(originalState);

    TaskCompletionSource<Unit> overlappingEntered = CreateSignal();
    TaskCompletionSource<Unit> successorCommitted = CreateSignal();
    TaskCompletionSource<Unit> releaseEarlier = CreateSignal();
    bool earlierInFlight = false;
    bool overlappingSawEarlierInFlight = false;
    TransactionTestState successorState = null!;

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> earlierNext = async (_, _) =>
    {
      await overlappingEntered.Task.WithTimeout();
      await harness.Behavior.Handle
      (
        new TransactionTestState.ThrowAction(),
        (_, _) =>
        {
          successorState = (TransactionTestState)harness.Store.CurrentState;
          successorState.Value = 77;
          return new ValueTask<Unit>(Unit.Value);
        },
        CancellationToken.None
      );
      earlierInFlight = true;
      successorCommitted.SetResult(Unit.Value);
      await releaseEarlier.Task.WithTimeout();
      earlierInFlight = false;
      return Unit.Value;
    };

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> overlappingNext = async (_, _) =>
    {
      overlappingEntered.SetResult(Unit.Value);
      await successorCommitted.Task.WithTimeout();
      overlappingSawEarlierInFlight = earlierInFlight;
      throw new InvalidOperationException("overlapping action failed");
    };

    Task earlier = harness.Behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      earlierNext,
      CancellationToken.None
    ).AsTask();
    Task overlapping = harness.Behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      overlappingNext,
      CancellationToken.None
    ).AsTask();
    await overlapping.WithTimeout();
    releaseEarlier.SetResult(Unit.Value);
    await earlier.WithTimeout();

    overlappingSawEarlierInFlight.ShouldBeTrue();
    earlierInFlight.ShouldBeFalse();
    harness.Store.CurrentState.ShouldBeSameAs(successorState);
    successorState.Value.ShouldBe(77);
    originalState.Value.ShouldBe(5);
    harness.Publisher.Publications.Count.ShouldBe(1);
    harness.Logger.Entries
      .Count(entry => entry.EventId == EventIds.StateTransactionBehavior_ConcurrentAdvance)
      .ShouldBe(1);
    harness.Logger.Entries.ShouldNotContain
    (
      entry => entry.EventId == EventIds.StateTransactionBehavior_Restoring
    );
  }

  public async Task Restore_Earlier_Clone_When_Overlapping_Action_Fails_While_Earlier_Action_Is_In_Flight()
  {
    TransactionTestState originalState = CreateOriginalState();
    Harness<TransactionTestState.ThrowAction> harness = CreateHarness(originalState);

    TaskCompletionSource<Unit> earlierEntered = CreateSignal();
    TaskCompletionSource<Unit> releaseEarlier = CreateSignal();
    bool earlierInFlight = false;
    bool overlappingSawEarlierInFlight = false;
    TransactionTestState earlierClone = null!;

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> earlierNext = async (_, _) =>
    {
      earlierClone = (TransactionTestState)harness.Store.CurrentState;
      earlierClone.Value = 3;
      earlierInFlight = true;
      earlierEntered.SetResult(Unit.Value);
      await releaseEarlier.Task.WithTimeout();
      TransactionTestState live = (TransactionTestState)harness.Store.CurrentState;
      live.Value = 11;
      earlierInFlight = false;
      return Unit.Value;
    };

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> overlappingNext = (_, _) =>
    {
      TransactionTestState current = (TransactionTestState)harness.Store.CurrentState;
      current.Value = 9;
      overlappingSawEarlierInFlight = earlierInFlight;
      return new ValueTask<Unit>(Task.FromException<Unit>(new InvalidOperationException("overlapping action failed")));
    };

    Task earlier = harness.Behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      earlierNext,
      CancellationToken.None
    ).AsTask();
    // The earlier handler runs synchronously up to its first await, so it has already signalled; the wait is a guard.
    await earlierEntered.Task.WithTimeout();
    Task overlapping = harness.Behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      overlappingNext,
      CancellationToken.None
    ).AsTask();
    await overlapping.WithTimeout();

    overlappingSawEarlierInFlight.ShouldBeTrue();
    earlierInFlight.ShouldBeTrue();
    harness.Store.CurrentState.ShouldBeSameAs(earlierClone);
    earlierClone.Value.ShouldBe(3);
    harness.Logger.Entries.ShouldContain
    (
      entry => entry.EventId == EventIds.StateTransactionBehavior_Restoring
    );
    harness.Logger.Entries.ShouldNotContain
    (
      entry => entry.EventId == EventIds.StateTransactionBehavior_ConcurrentAdvance
    );

    releaseEarlier.SetResult(Unit.Value);
    await earlier.WithTimeout();

    harness.Store.CurrentState.ShouldBeSameAs(earlierClone);
    earlierClone.Value.ShouldBe(11);
    originalState.Value.ShouldBe(5);
    harness.Store.CurrentState.ShouldNotBeSameAs(originalState);
    harness.Publisher.Publications.Count.ShouldBe(1);
  }
}
