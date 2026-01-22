# Task 057: Add RenderSubscriptionContext Integration Tests

## Description

- Add integration tests for RenderSubscriptionContext
- Controls whether subscriptions fire for specific actions

## Requirements

- Use real RenderSubscriptionContext from DI
- Add global using for TimeWarp.Features.RenderSubscriptions

## Checklist

- [x] Create `render-subscription-context-tests.cs` in `pipeline/` directory with 4 tests:
  - [x] ReturnTrue_ForUnregisteredAction
  - [x] ReturnFalse_WhenRegisteredWithFalse
  - [x] ReturnTrue_WhenRegisteredWithTrue
  - [x] ReturnTrue_AfterReset
- [x] Add `global using TimeWarp.Features.RenderSubscriptions;` to global-usings.cs
- [x] Verify all tests pass

## Notes

Reference: `.agent/workspace/2025-12-04T16-30-00_integration-tests-implementation.md`

## Implementation Notes

- Added RenderSubscriptionContext property to BaseTest
- All 4 tests passing

