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

- [ ] Create `tests/client-integration-tests/subscriptions/` directory
- [ ] Create `subscriptions-tests.cs` with 6 tests:
  - [ ] AddSubscription_AndTriggerReRender
  - [ ] NotAddDuplicate_ForSameComponentAndState
  - [ ] RemoveAllSubscriptions_ForComponent
  - [ ] RespectShouldReRender_ReturnValue
  - [ ] OnlyReRender_MatchingStateType
  - [ ] ReRenderMultipleComponents_ForSameState
- [ ] Verify all tests pass

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

