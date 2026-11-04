#region Purpose
// Proves StateTransactionBehavior rolls back on handler failure, publishes ExceptionNotification with
// CancellationToken.None for non-cancellation errors, rethrows OperationCanceledException without a
// notification, rethrows other handler exceptions only when RethrowHandlerExceptions is set, and
// leaves a newer clone in place when an overlapping action fails.
#endregion

#region Design
// Constructs StateTransactionBehavior directly over a RecordingStore and RecordingPublisher. TransactionTestState
// implements ICloneable so the clone is predictable; next mutates the current state and then fails, so a rollback
// shows up as the original instance with its original value. EqualGuidState.Clone is MemberwiseClone, so Guid is
// copied. EmptyGuidState.Clone returns Guid.Empty. ThrowingConstructorState is not ICloneable and not a State<T>,
// so the registry has no generated clone and Clone throws InvalidOperationException.
// Overlap tests share one behavior instance and interleave two next delegates with TaskCompletionSource. The store
// reference, not a timer, decides whether a failure restores its snapshot.
// Keep_Injected_Services_Across_Two_Actions uses the generated clone of a [CloneShared] service state. Two successful
// actions must leave the same non-null logger, HttpClient, and NavigationManager on the live state.
#endregion

namespace StateTransactionBehaviorTests;

public partial class Should_
{
  public async Task Rollback_And_Publish_Notification_When_Handler_Throws_On_Cancelled_Token()
  {
    Harness harness = CreateHarness();
    using CancellationTokenSource cancellationTokenSource = new();
    cancellationTokenSource.Cancel();

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> next = (_, _) =>
    {
      var current = (TransactionTestState)harness.Store.CurrentState;
      current.Value = 99;
      return new ValueTask<Unit>(Task.FromException<Unit>(new InvalidOperationException("handler failed")));
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

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> next = (_, _) =>
    {
      var current = (TransactionTestState)harness.Store.CurrentState;
      current.Value = 99;
      return new ValueTask<Unit>(Task.FromException<Unit>(new OperationCanceledException()));
    };

    await Should.ThrowAsync<OperationCanceledException>
    (
      async () => await harness.Behavior.Handle
      (
        new TransactionTestState.ThrowAction(),
        next,
        CancellationToken.None
      )
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

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> next = (_, cancellationToken) =>
    {
      var current = (TransactionTestState)harness.Store.CurrentState;
      current.Value = 99;
      return new ValueTask<Unit>(Task.FromException<Unit>(new OperationCanceledException(cancellationToken)));
    };

    await Should.ThrowAsync<OperationCanceledException>
    (
      async () => await harness.Behavior.Handle
      (
        new TransactionTestState.ThrowAction(),
        next,
        cancellationTokenSource.Token
      )
    );

    harness.Store.CurrentState.ShouldBeSameAs(harness.OriginalState);
    harness.OriginalState.Value.ShouldBe(5);
    harness.Publisher.Publications.ShouldBeEmpty();
  }

  public async Task Rethrow_Handler_Exception_After_Notification_When_Option_Is_Set()
  {
    TransactionTestState originalState = new(Guid.NewGuid(), value: 5);
    RecordingStore recordingStore = new(originalState);
    RecordingPublisher recordingPublisher = new();
    ServiceCollection serviceCollection = new();
    TimeWarpStateOptions options = new(serviceCollection)
    {
      RethrowHandlerExceptions = true
    };
    StateTransactionBehavior<TransactionTestState.ThrowAction, Unit> behavior = new
    (
      NullLogger<StateTransactionBehavior<TransactionTestState.ThrowAction, Unit>>.Instance,
      recordingStore,
      recordingPublisher,
      options
    );

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> next = (_, _) =>
    {
      var current = (TransactionTestState)recordingStore.CurrentState;
      current.Value = 99;
      return new ValueTask<Unit>(Task.FromException<Unit>(new InvalidOperationException("handler failed")));
    };

    InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>
    (
      async () => await behavior.Handle(new TransactionTestState.ThrowAction(), next, CancellationToken.None)
    );

    exception.Message.ShouldBe("handler failed");
    recordingStore.CurrentState.ShouldBeSameAs(originalState);
    originalState.Value.ShouldBe(5);
    recordingPublisher.Publications.Count.ShouldBe(1);
    recordingPublisher.Publications[0].CancellationToken.ShouldBe(CancellationToken.None);
  }

  public async Task Throw_InvalidCloneException_When_Clone_Copies_Guid()
  {
    Guid originalGuid = Guid.NewGuid();
    EqualGuidState originalState = new(originalGuid);
    StateTransactionBehavior<EqualGuidState.ThrowAction, Unit> behavior = CreateBehavior<EqualGuidState.ThrowAction>(originalState);

    InvalidCloneException exception = await Should.ThrowAsync<InvalidCloneException>
    (
      async () => await behavior.Handle
      (
        new EqualGuidState.ThrowAction(),
        (_, _) => new ValueTask<Unit>(Unit.Value),
        CancellationToken.None
      )
    );

    exception.EnclosingStateType.ShouldBe(typeof(EqualGuidState));
    exception.CloneCause.ShouldBe(InvalidCloneException.Cause.EqualGuid);
    exception.Message.ShouldContain("equal Guid");
    exception.Message.ShouldContain("[CloneShared]");
  }

  public async Task Throw_InvalidCloneException_When_Clone_Guid_Is_Empty()
  {
    EmptyGuidState originalState = new(Guid.NewGuid());
    StateTransactionBehavior<EmptyGuidState.ThrowAction, Unit> behavior = CreateBehavior<EmptyGuidState.ThrowAction>(originalState);

    InvalidCloneException exception = await Should.ThrowAsync<InvalidCloneException>
    (
      async () => await behavior.Handle
      (
        new EmptyGuidState.ThrowAction(),
        (_, _) => new ValueTask<Unit>(Unit.Value),
        CancellationToken.None
      )
    );

    exception.EnclosingStateType.ShouldBe(typeof(EmptyGuidState));
    exception.CloneCause.ShouldBe(InvalidCloneException.Cause.EmptyGuid);
    exception.Message.ShouldContain("empty Guid");
    exception.Message.ShouldContain("construct the clone so the initializer runs");
    exception.Message.ShouldContain("[CloneShared]");
  }

  public async Task Throw_When_State_Has_No_Clone()
  {
    ThrowingConstructorState originalState = new(seed: 1);
    originalState.Guid.ShouldNotBe(Guid.Empty);
    StateTransactionBehavior<ThrowingConstructorState.ThrowAction, Unit> behavior =
      CreateBehavior<ThrowingConstructorState.ThrowAction>(originalState);

    InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>
    (
      async () => await behavior.Handle
      (
        new ThrowingConstructorState.ThrowAction(),
        (_, _) => new ValueTask<Unit>(Unit.Value),
        CancellationToken.None
      )
    );

    exception.Message.ShouldContain(nameof(ThrowingConstructorState));
    exception.Message.ShouldContain("ICloneable");
  }

  public async Task Keep_Later_Commit_When_Earlier_Action_Fails_After_Later_Action_Commits()
  {
    TransactionTestState originalState = new(Guid.NewGuid(), value: 5);
    RecordingStore store = new(originalState);
    RecordingPublisher publisher = new();
    RecordingLogger<StateTransactionBehavior<TransactionTestState.ThrowAction, Unit>> logger = new();
    StateTransactionBehavior<TransactionTestState.ThrowAction, Unit> behavior =
      CreateThrowBehavior(store, publisher, logger);

    TaskCompletionSource<Unit> laterCommitted = new();
    TransactionTestState laterState = null!;

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> earlierNext = async (_, _) =>
    {
      await laterCommitted.Task;
      throw new InvalidOperationException("earlier action failed");
    };

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> laterNext = (_, _) =>
    {
      laterState = (TransactionTestState)store.CurrentState;
      laterState.Value = 42;
      laterCommitted.SetResult(Unit.Value);
      return new ValueTask<Unit>(Unit.Value);
    };

    Task earlier = behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      earlierNext,
      CancellationToken.None
    ).AsTask();
    Task later = behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      laterNext,
      CancellationToken.None
    ).AsTask();
    await Task.WhenAll(earlier, later);

    store.CurrentState.ShouldBeSameAs(laterState);
    laterState.Value.ShouldBe(42);
    originalState.Value.ShouldBe(5);
    store.CurrentState.ShouldNotBeSameAs(originalState);
    publisher.Publications.Count.ShouldBe(1);
    publisher.Publications[0].CancellationToken.ShouldBe(CancellationToken.None);
    logger.Entries.ShouldContain
    (
      entry => entry.EventId == EventIds.StateTransactionBehavior_ConcurrentAdvance
    );
    logger.Entries.ShouldNotContain
    (
      entry => entry.EventId == EventIds.StateTransactionBehavior_Restoring
    );
  }

  public async Task Keep_Successor_Commit_When_Overlapping_Action_Fails_While_Earlier_Action_Is_In_Flight()
  {
    TransactionTestState originalState = new(Guid.NewGuid(), value: 5);
    RecordingStore store = new(originalState);
    RecordingPublisher publisher = new();
    RecordingLogger<StateTransactionBehavior<TransactionTestState.ThrowAction, Unit>> logger = new();
    StateTransactionBehavior<TransactionTestState.ThrowAction, Unit> behavior =
      CreateThrowBehavior(store, publisher, logger);

    TaskCompletionSource<Unit> overlappingEntered = new();
    TaskCompletionSource<Unit> successorCommitted = new();
    TaskCompletionSource<Unit> releaseEarlier = new();
    bool earlierInFlight = false;
    bool overlappingSawEarlierInFlight = false;
    TransactionTestState successorState = null!;

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> earlierNext = async (_, _) =>
    {
      await overlappingEntered.Task;
      await behavior.Handle
      (
        new TransactionTestState.ThrowAction(),
        (_, _) =>
        {
          successorState = (TransactionTestState)store.CurrentState;
          successorState.Value = 77;
          return new ValueTask<Unit>(Unit.Value);
        },
        CancellationToken.None
      );
      earlierInFlight = true;
      successorCommitted.SetResult(Unit.Value);
      await releaseEarlier.Task;
      earlierInFlight = false;
      return Unit.Value;
    };

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> overlappingNext = async (_, _) =>
    {
      overlappingEntered.SetResult(Unit.Value);
      await successorCommitted.Task;
      overlappingSawEarlierInFlight = earlierInFlight;
      throw new InvalidOperationException("overlapping action failed");
    };

    Task earlier = behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      earlierNext,
      CancellationToken.None
    ).AsTask();
    Task overlapping = behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      overlappingNext,
      CancellationToken.None
    ).AsTask();
    await overlapping;
    releaseEarlier.SetResult(Unit.Value);
    await earlier;

    overlappingSawEarlierInFlight.ShouldBeTrue();
    earlierInFlight.ShouldBeFalse();
    store.CurrentState.ShouldBeSameAs(successorState);
    successorState.Value.ShouldBe(77);
    originalState.Value.ShouldBe(5);
    publisher.Publications.Count.ShouldBe(1);
    logger.Entries
      .Count(entry => entry.EventId == EventIds.StateTransactionBehavior_ConcurrentAdvance)
      .ShouldBe(1);
    logger.Entries.ShouldNotContain
    (
      entry => entry.EventId == EventIds.StateTransactionBehavior_Restoring
    );
  }

  public async Task Restore_Earlier_Clone_When_Overlapping_Action_Fails_While_Earlier_Action_Is_In_Flight()
  {
    TransactionTestState originalState = new(Guid.NewGuid(), value: 5);
    RecordingStore store = new(originalState);
    RecordingPublisher publisher = new();
    RecordingLogger<StateTransactionBehavior<TransactionTestState.ThrowAction, Unit>> logger = new();
    StateTransactionBehavior<TransactionTestState.ThrowAction, Unit> behavior =
      CreateThrowBehavior(store, publisher, logger);

    TaskCompletionSource<Unit> releaseEarlier = new();
    bool earlierInFlight = false;
    bool overlappingSawEarlierInFlight = false;
    TransactionTestState earlierClone = null!;

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> earlierNext = async (_, _) =>
    {
      earlierClone = (TransactionTestState)store.CurrentState;
      earlierClone.Value = 3;
      earlierInFlight = true;
      await releaseEarlier.Task;
      TransactionTestState live = (TransactionTestState)store.CurrentState;
      live.Value = 11;
      earlierInFlight = false;
      return Unit.Value;
    };

    MessageHandlerDelegate<TransactionTestState.ThrowAction, Unit> overlappingNext = (_, _) =>
    {
      TransactionTestState current = (TransactionTestState)store.CurrentState;
      current.Value = 9;
      overlappingSawEarlierInFlight = earlierInFlight;
      return new ValueTask<Unit>(Task.FromException<Unit>(new InvalidOperationException("overlapping action failed")));
    };

    Task earlier = behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      earlierNext,
      CancellationToken.None
    ).AsTask();
    Task overlapping = behavior.Handle
    (
      new TransactionTestState.ThrowAction(),
      overlappingNext,
      CancellationToken.None
    ).AsTask();
    await overlapping;

    overlappingSawEarlierInFlight.ShouldBeTrue();
    earlierInFlight.ShouldBeTrue();
    store.CurrentState.ShouldBeSameAs(earlierClone);
    earlierClone.Value.ShouldBe(3);
    logger.Entries.ShouldContain
    (
      entry => entry.EventId == EventIds.StateTransactionBehavior_Restoring
    );
    logger.Entries.ShouldNotContain
    (
      entry => entry.EventId == EventIds.StateTransactionBehavior_ConcurrentAdvance
    );

    releaseEarlier.SetResult(Unit.Value);
    await earlier;

    store.CurrentState.ShouldBeSameAs(earlierClone);
    earlierClone.Value.ShouldBe(11);
    originalState.Value.ShouldBe(5);
    store.CurrentState.ShouldNotBeSameAs(originalState);
    publisher.Publications.Count.ShouldBe(1);
  }

  public async Task Keep_Injected_Services_Across_Two_Actions()
  {
    ILogger<ServiceState> logger = NullLogger<ServiceState>.Instance;
    using HttpClient httpClient = new();
    NavigationManager navigationManager = new TestNavigationManager();
    ServiceState original = new(logger, httpClient, navigationManager);
    RecordingStore store = new(original);
    RecordingPublisher publisher = new();
    StateTransactionBehavior<ServiceState.TickAction, Unit> behavior = new
    (
      NullLogger<StateTransactionBehavior<ServiceState.TickAction, Unit>>.Instance,
      store,
      publisher,
      new TimeWarpStateOptions(new ServiceCollection())
    );

    MessageHandlerDelegate<ServiceState.TickAction, Unit> next = (_, _) =>
    {
      ServiceState current = (ServiceState)store.GetState(typeof(ServiceState));
      current.Logger.LogInformation("tick {Count}", current.Count);
      current.NavigationManager.BaseUri.ShouldBe("https://example.test/");
      current.HttpClient.ShouldBeSameAs(httpClient);
      current.Count++;
      return new ValueTask<Unit>(Unit.Value);
    };

    await behavior.Handle(new ServiceState.TickAction(), next, CancellationToken.None);
    ServiceState afterFirst = (ServiceState)store.CurrentState;
    afterFirst.ShouldNotBeSameAs(original);
    afterFirst.Count.ShouldBe(1);
    afterFirst.Logger.ShouldBeSameAs(logger);
    afterFirst.HttpClient.ShouldBeSameAs(httpClient);
    afterFirst.NavigationManager.ShouldBeSameAs(navigationManager);

    await behavior.Handle(new ServiceState.TickAction(), next, CancellationToken.None);
    ServiceState afterSecond = (ServiceState)store.CurrentState;
    afterSecond.ShouldNotBeSameAs(afterFirst);
    afterSecond.ShouldNotBeSameAs(original);
    afterSecond.Count.ShouldBe(2);
    afterSecond.Logger.ShouldBeSameAs(logger);
    afterSecond.HttpClient.ShouldBeSameAs(httpClient);
    afterSecond.NavigationManager.ShouldBeSameAs(navigationManager);
    afterSecond.Guid.ShouldNotBe(original.Guid);
    afterSecond.Guid.ShouldNotBe(afterFirst.Guid);
    publisher.Publications.ShouldBeEmpty();
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

  private static StateTransactionBehavior<TransactionTestState.ThrowAction, Unit> CreateThrowBehavior
  (
    RecordingStore store,
    RecordingPublisher publisher,
    ILogger<StateTransactionBehavior<TransactionTestState.ThrowAction, Unit>> logger
  )
  {
    return new StateTransactionBehavior<TransactionTestState.ThrowAction, Unit>
    (
      logger,
      store,
      publisher,
      new TimeWarpStateOptions(new ServiceCollection())
    );
  }

}
