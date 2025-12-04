# Task 052: Add Store Lifecycle Integration Tests

## Description

- Add integration tests for Store state lifecycle management
- Tests use real Store via existing ClientHost infrastructure - no mocks
- Covers: GetState, Reset, RemoveState, PreviousState tracking

## Requirements

- Extend existing `BaseTest` class
- Use real Store, real states (CounterState, BlueState)
- No mocks of code we control

## Checklist

- [ ] Create `tests/client-integration-tests/store/` directory
- [ ] Create `store-lifecycle-tests.cs` with 8 tests:
  - [ ] ReturnSameStateInstance_OnRepeatedCalls
  - [ ] ReturnDifferentInstances_ForDifferentStateTypes
  - [ ] InitializeState_OnFirstAccess
  - [ ] CreateNewInstance_AfterReset
  - [ ] CreateNewInstance_AfterRemoveState
  - [ ] ReturnNullPreviousState_BeforeAnyAction
  - [ ] TrackPreviousState_AfterAction
  - [ ] HaveNewGuid_AfterAction
- [ ] Add `global using Test.App.Client.Features.Blue;` to global-usings.cs
- [ ] Verify all tests pass

## Notes

Reference: `.agent/workspace/2025-12-04T16-30-00_integration-tests-implementation.md`

## Implementation Notes

