# Task 053: Add Subscriptions Integration Tests

## Description

- Add integration tests for the Subscriptions system
- Currently has **zero test coverage** - critical gap
- Tests use real Subscriptions from DI

## Requirements

- Use real Subscriptions instance from ServiceProvider
- One minimal test double: `TestableComponent` implementing `ITimeWarpStateComponent` (8 lines)
- No mocks of TimeWarp.State code

## Checklist

- [x] Create `tests/client-integration-tests/subscriptions/` directory
- [x] Create `subscriptions-tests.cs` with 6 tests:
  - [x] AddSubscription_AndTriggerReRender
  - [x] NotAddDuplicate_ForSameComponentAndState
  - [x] RemoveAllSubscriptions_ForComponent
  - [x] RespectShouldReRender_ReturnValue
  - [x] OnlyReRender_MatchingStateType
  - [x] ReRenderMultipleComponents_ForSameState
- [x] Verify all tests pass

## Notes

TestableComponent (only test double needed):
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

Reference: `.agent/workspace/2025-12-04T16-30-00_integration-tests-implementation.md`

## Implementation Notes

- Added Subscriptions to BaseTest class for easy access in tests
- Added Counter namespace to global-usings.cs
- All 6 tests passing
