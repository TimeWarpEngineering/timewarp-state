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

- [x] Create `tests/client-integration-tests/store/` directory
- [x] Create `store-lifecycle-tests.cs` with 8 tests:
  - [x] ReturnSameStateInstance_OnRepeatedCalls
  - [x] ReturnDifferentInstances_ForDifferentStateTypes
  - [x] InitializeState_OnFirstAccess
  - [x] CreateNewInstance_AfterReset
  - [x] CreateNewInstance_AfterRemoveState
  - [x] ReturnNullPreviousState_BeforeAnyAction
  - [x] TrackPreviousState_AfterAction
  - [x] HaveNewGuid_AfterAction
- [x] Add `global using Test.App.Client.Features.Blue;` to global-usings.cs
- [x] Verify all tests pass

## Notes

Reference: `.agent/workspace/2025-12-04T16-30-00_integration-tests-implementation.md`

## Implementation Notes

All 8 tests passing as of implementation.

