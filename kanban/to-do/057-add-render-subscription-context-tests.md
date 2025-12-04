# Task 057: Add RenderSubscriptionContext Integration Tests

## Description

- Add integration tests for RenderSubscriptionContext
- Controls whether subscriptions fire for specific actions

## Requirements

- Use real RenderSubscriptionContext from DI
- Add global using for TimeWarp.Features.RenderSubscriptions

## Checklist

- [ ] Create `render-subscription-context-tests.cs` in `pipeline/` directory with 4 tests:
  - [ ] ReturnTrue_ForUnregisteredAction
  - [ ] ReturnFalse_WhenRegisteredWithFalse
  - [ ] ReturnTrue_WhenRegisteredWithTrue
  - [ ] ReturnTrue_AfterReset
- [ ] Add `global using TimeWarp.Features.RenderSubscriptions;` to global-usings.cs
- [ ] Verify all tests pass

## Notes

Reference: `.agent/workspace/2025-12-04T16-30-00_integration-tests-implementation.md`

## Implementation Notes

