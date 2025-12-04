# Task 052: Add Integration Test Coverage

## Description

- Add integration tests for core TimeWarp.State functionality
- Current coverage is ~35% with heavy reliance on slow E2E tests
- Tests should use real infrastructure (no mocks of code we control)
- Focus on P0 (critical) and P1 (high priority) gaps identified in coverage analysis

## Requirements

- All tests use real Store, Subscriptions, ISender - no mocks
- Tests extend existing `BaseTest` and use `ClientHost` infrastructure
- Only one minimal test double allowed: `TestableComponent` implementing `ITimeWarpStateComponent`
- Tests verify real use cases, not implementation details

## Checklist

### P0 - Critical (19 tests)

#### Store Lifecycle Tests (`store/store-lifecycle-tests.cs`)
- [ ] ReturnSameStateInstance_OnRepeatedCalls
- [ ] ReturnDifferentInstances_ForDifferentStateTypes
- [ ] InitializeState_OnFirstAccess
- [ ] CreateNewInstance_AfterReset
- [ ] CreateNewInstance_AfterRemoveState
- [ ] ReturnNullPreviousState_BeforeAnyAction
- [ ] TrackPreviousState_AfterAction
- [ ] HaveNewGuid_AfterAction

#### Subscriptions Tests (`subscriptions/subscriptions-tests.cs`)
- [ ] AddSubscription_AndTriggerReRender
- [ ] NotAddDuplicate_ForSameComponentAndState
- [ ] RemoveAllSubscriptions_ForComponent
- [ ] RespectShouldReRender_ReturnValue
- [ ] OnlyReRender_MatchingStateType
- [ ] ReRenderMultipleComponents_ForSameState

#### State Transaction Tests (`pipeline/state-transaction-tests.cs`)
- [ ] CloneState_BeforeHandlerExecutes
- [ ] PreserveChanges_OnSuccessfulAction
- [ ] RollbackState_OnException
- [ ] PublishExceptionNotification_OnException
- [ ] ContinueWorking_AfterException

### P1 - High Priority (12 tests)

#### Action Tracking Tests (`pipeline/action-tracking-tests.cs`)
- [ ] NotBeActive_Initially
- [ ] TrackAction_DuringExecution
- [ ] ReturnFalse_ForUnknownActionType
- [ ] TrackMultipleActions_Independently

#### Cacheable State Tests (`caching/cacheable-state-tests.cs`)
- [ ] HaveNullCacheKey_Initially
- [ ] SetCacheKey_AfterFetch
- [ ] SetTimestamp_AfterFetch
- [ ] ReturnCachedData_WhenCacheValid

#### RenderSubscriptionContext Tests (`pipeline/render-subscription-context-tests.cs`)
- [ ] ReturnTrue_ForUnregisteredAction
- [ ] ReturnFalse_WhenRegisteredWithFalse
- [ ] ReturnTrue_WhenRegisteredWithTrue
- [ ] ReturnTrue_AfterReset

### Setup
- [ ] Add global usings to client-integration-tests
- [ ] Create directory structure (store/, subscriptions/, pipeline/, caching/)

### Verification
- [ ] All 31 tests pass
- [ ] Run existing tests to ensure no regressions

## Notes

### Coverage Analysis Summary

| Area | Before | After |
|------|--------|-------|
| Store lifecycle | Indirect (E2E) | Direct integration |
| Subscriptions | **None** | Direct integration |
| State transactions | E2E only | Direct integration |
| Action tracking | E2E only | Direct integration |
| Caching | E2E only | Direct integration |
| Overall API coverage | ~35% | ~65% |

### File Structure

```
tests/client-integration-tests/
├── store/
│   └── store-lifecycle-tests.cs (8 tests)
├── subscriptions/
│   └── subscriptions-tests.cs (6 tests)
├── pipeline/
│   ├── state-transaction-tests.cs (5 tests)
│   ├── action-tracking-tests.cs (4 tests)
│   └── render-subscription-context-tests.cs (4 tests)
└── caching/
    └── cacheable-state-tests.cs (4 tests)
```

### Global Usings to Add

```csharp
global using TimeWarp.Features.RenderSubscriptions;
global using Test.App.Client.Features.Blue;
global using TimeWarp.State.Plus;
```

### TestableComponent (only test double needed)

```csharp
private class TestableComponent : ITimeWarpStateComponent
{
  public string Id { get; }
  public int ReRenderCount { get; private set; }
  public bool ShouldReRenderValue { get; set; } = true;

  public TestableComponent(string id) => Id = id;
  public void ReRender() => ReRenderCount++;
  public bool ShouldReRender(Type stateType) => ShouldReRenderValue;
}
```

### Reference Documents

- `.agent/workspace/2025-12-04T16-00-00_comprehensive-test-coverage-analysis.md`
- `.agent/workspace/2025-12-04T16-30-00_integration-tests-implementation.md`

## Implementation Notes

