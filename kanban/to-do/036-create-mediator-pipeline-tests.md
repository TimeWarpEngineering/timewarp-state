# Task 036: Create Mediator Pipeline Tests Before Migration

## Description

- Create unit and integration tests for the mediator pipeline components before migrating to martinothamar/Mediator
- Current test coverage for pipeline behaviors, pre/post processors, and Store is essentially non-existent
- These tests will serve as regression tests during and after migration
- See `.agent/workspace/2025-12-04T10-00-00_test-coverage-analysis.md` for full analysis

## Requirements

- Create test infrastructure (fakes/mocks for Store, Sender, Publisher)
- Add unit tests for StateTransactionBehavior
- Add unit tests for pre/post processors
- Add unit tests for Store class
- Add integration tests for pipeline execution order
- All new tests must pass before migration begins

## Checklist

### Phase 1: Test Infrastructure

#### Create Test Doubles (`tests/timewarp-state-tests/test-infrastructure/`)
- [ ] Create `FakeStore.cs` implementing `IStore`
  - [ ] Track `GetState` calls
  - [ ] Track `SetState` calls with state history
  - [ ] Track `GetPreviousState` calls
  - [ ] Support semaphore management
- [ ] Create `FakeSender.cs` implementing `ISender`
  - [ ] Capture sent requests for verification
  - [ ] Support configurable responses
- [ ] Create `FakePublisher.cs` implementing `IPublisher`
  - [ ] Capture published notifications
  - [ ] Track publish order
- [ ] Create `TestState.cs` - Simple test state class
- [ ] Create `TestAction.cs` - Simple test action

### Phase 2: Store Unit Tests

#### Create `tests/timewarp-state-tests/store/store-tests.cs`
- [ ] `GetState_WhenStateDoesNotExist_CreatesNewState`
- [ ] `GetState_WhenStateDoesNotExist_InitializesState`
- [ ] `GetState_WhenStateDoesNotExist_SetsSenderOnState`
- [ ] `GetState_WhenStateExists_ReturnsSameInstance`
- [ ] `GetState_PublishesStateInitializedNotification`
- [ ] `GetState_AddsInitializationTaskToDictionary`
- [ ] `SetState_UpdatesCurrentState`
- [ ] `SetState_TracksPreviousState`
- [ ] `SetState_WhenStateWasRemoved_DoesNotUpdate`
- [ ] `GetPreviousState_ReturnsCorrectPreviousState`
- [ ] `GetPreviousState_WhenNoPrevious_ReturnsNull`
- [ ] `RemoveState_RemovesFromDictionary`
- [ ] `RemoveState_CallsCancelOperationsOnState`
- [ ] `RemoveState_DisposesSemaphore`
- [ ] `RemoveState_RemovesInitializationTask`
- [ ] `GetSemaphore_WhenStateExists_CreatesSemaphore`
- [ ] `GetSemaphore_WhenStateDoesNotExist_ReturnsNull`
- [ ] `Reset_ClearsAllStates`

### Phase 3: Pipeline Behavior Tests

#### Create `tests/timewarp-state-tests/pipeline/state-transaction-behavior-tests.cs`
- [ ] `Handle_ClonesStateBeforeCallingNext`
- [ ] `Handle_SetsClonedStateInStore`
- [ ] `Handle_ClonedStateHasDifferentGuid`
- [ ] `Handle_PreservesSenderOnClonedState`
- [ ] `Handle_OnSuccess_ReturnsResponseFromNext`
- [ ] `Handle_OnException_RestoresOriginalState`
- [ ] `Handle_OnException_PublishesExceptionNotification`
- [ ] `Handle_OnException_ReturnsDefault`
- [ ] `Handle_ThrowsInvalidCloneException_WhenGuidEmpty`
- [ ] `Handle_ThrowsInvalidCloneException_WhenGuidSameAsOriginal`

### Phase 4: Pre/Post Processor Tests

#### Create `tests/timewarp-state-tests/processors/state-initialization-pre-processor-tests.cs`
- [ ] `Process_WhenInitializationTaskExists_WaitsForCompletion`
- [ ] `Process_WhenNoInitializationTask_CompletesImmediately`
- [ ] `Process_WhenTaskFails_PropagatesException`
- [ ] `Process_GetsEnclosingStateTypeFromRequest`

#### Create `tests/timewarp-state-tests/processors/render-subscriptions-post-processor-tests.cs`
- [ ] `Process_CallsReRenderSubscribers`
- [ ] `Process_GetsEnclosingStateTypeFromRequest`
- [ ] `Process_WhenShouldNotFireSubscriptions_SkipsReRender`
- [ ] `Process_WhenShouldNotFireSubscriptions_LogsSkip`
- [ ] `Process_OnException_Throws`

### Phase 5: Subscriptions Tests

#### Create `tests/timewarp-state-tests/subscriptions/subscriptions-tests.cs`
- [ ] `Add_AddsNewSubscription`
- [ ] `Add_DoesNotAddDuplicateSubscription`
- [ ] `Remove_RemovesAllSubscriptionsForComponent`
- [ ] `ReRenderSubscribers_CallsReRenderOnSubscribedComponents`
- [ ] `ReRenderSubscribers_OnlyReRendersMatchingStateType`
- [ ] `ReRenderSubscribers_RemovesDeadWeakReferences`
- [ ] `ReRenderSubscribers_SkipsComponentsWhereShouldReRenderFalse`

#### Create `tests/timewarp-state-tests/subscriptions/render-subscription-context-tests.cs`
- [ ] `EnsureAction_RegistersAction`
- [ ] `ShouldFireSubscriptionsForAction_WhenNotRegistered_ReturnsTrue`
- [ ] `ShouldFireSubscriptionsForAction_WhenRegisteredWithFalse_ReturnsFalse`
- [ ] `ShouldFireSubscriptionsForAction_WhenRegisteredWithTrue_ReturnsTrue`
- [ ] `Reset_ClearsAllRegistrations`
- [ ] `RemoveAction_RemovesSpecificAction`

### Phase 6: Integration Tests

#### Create `tests/timewarp-state-tests/integration/pipeline-execution-order-tests.cs`
- [ ] `Pipeline_ExecutesPreProcessorBeforeHandler`
- [ ] `Pipeline_ExecutesPostProcessorAfterHandler`
- [ ] `Pipeline_ExecutesBehaviorsInOrder`
- [ ] `Pipeline_PassesRequestThroughEntirePipeline`
- [ ] `Pipeline_OnHandlerException_StillExecutesExceptionHandling`

## Notes

### Current Coverage Gap Summary

| Component | Current Tests | Needed Tests |
|-----------|--------------|--------------|
| Store | 0 | ~18 |
| StateTransactionBehavior | 0 | ~10 |
| Pre/Post Processors | 0 | ~9 |
| Subscriptions | 0 | ~13 |
| Pipeline Integration | 0 | ~5 |
| **Total** | **0** | **~55** |

### Test File Structure

```
tests/timewarp-state-tests/
├── test-infrastructure/
│   ├── FakeStore.cs
│   ├── FakeSender.cs
│   ├── FakePublisher.cs
│   ├── TestState.cs
│   └── TestAction.cs
├── store/
│   └── store-tests.cs
├── pipeline/
│   └── state-transaction-behavior-tests.cs
├── processors/
│   ├── state-initialization-pre-processor-tests.cs
│   └── render-subscriptions-post-processor-tests.cs
├── subscriptions/
│   ├── subscriptions-tests.cs
│   └── render-subscription-context-tests.cs
└── integration/
    └── pipeline-execution-order-tests.cs
```

### Why These Tests Matter for Migration

1. **StateTransactionBehavior** - Uses `RequestHandlerDelegate<TResponse>` which changes to `MessageHandlerDelegate<TRequest, TResponse>`
2. **Pre/Post Processors** - Change from interfaces to abstract classes
3. **Store** - Uses `IPublisher` which stays the same but tests verify behavior
4. **Pipeline Order** - Ensures behaviors execute in correct order after migration

### Dependencies

This task should be completed **BEFORE** Task 037 (Test Baseline) and definitely before any migration tasks (038+).

## Implementation Notes

### Example Test Structure

```csharp
namespace StoreTests;

public class GetState_Should
{
  private readonly FakePublisher _publisher;
  private readonly FakeServiceProvider _serviceProvider;
  private readonly Store _sut;

  public GetState_Should()
  {
    _publisher = new FakePublisher();
    _serviceProvider = new FakeServiceProvider();
    _sut = new Store(
      NullLogger<Store>.Instance,
      _serviceProvider,
      new TimeWarpStateOptions(new ServiceCollection()),
      _publisher
    );
  }

  public void CreateNewState_WhenStateDoesNotExist()
  {
    // Arrange
    // (state doesn't exist yet)

    // Act
    var state = _sut.GetState<TestState>();

    // Assert
    state.ShouldNotBeNull();
  }
}
```

### Running Tests

```bash
# Run just the new tests
dotnet fixie timewarp-state-tests --filter StoreTests

# Run all tests
./scripts/test.cs
```
