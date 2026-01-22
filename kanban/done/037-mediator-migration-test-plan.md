# Task 037: Mediator Migration - Establish Test Baseline and Test Plan

**STATUS: CANCELLED**

## Reason for Cancellation

This task was superseded by tasks 052-057 which added comprehensive integration test coverage:
- Task 052: Store lifecycle tests (8 tests)
- Task 053: Subscriptions tests (6 tests)
- Task 054: State transaction tests (5 tests)
- Task 055: Action tracking tests (4 tests)
- Task 056: Cacheable state tests (4 tests)
- Task 057: RenderSubscriptionContext tests (4 tests)

The remaining baseline verification work is implicit in the migration tasks - tests will be run as part of each migration step.

---

## Original Description

- Establish a baseline by running all existing tests before the migration
- Document current test coverage for mediator-related functionality
- Create a test plan to verify the migration doesn't introduce regressions
- Identify any gaps in test coverage that should be addressed before migration

## Original Requirements

- All existing tests must pass before migration begins
- Document baseline test results
- Identify critical paths that must be tested
- Create checklist for post-migration verification

## Checklist

### Pre-Migration Baseline

#### Run All Test Suites
- [ ] Run unit tests: `./scripts/test.cs`
  - [ ] timewarp-state-analyzer-tests
  - [ ] timewarp-state-tests
  - [ ] timewarp-state-plus-tests
  - [ ] client-integration-tests
  - [ ] test-app-architecture-tests
- [ ] Run E2E tests: `./scripts/e2e.cs`
  - [ ] counter-page-test
  - [ ] event-stream-page-tests
  - [ ] throw-exception-page-tests
  - [ ] persistence-test-page-tests
  - [ ] reset-store-page-tests
  - [ ] change-route-page-tests
  - [ ] go-back-page-tests
  - [ ] javascript-interop-page-tests
- [ ] Document baseline results (pass/fail counts, any known failures)

#### Verify Build
- [ ] Run `dotnet build` on entire solution
- [ ] Run `./scripts/build.cs`
- [ ] Run `./scripts/package.cs` to verify NuGet package creation

### Critical Mediator Functionality to Test

#### 1. Pipeline Behavior Execution Order
Test that pipeline behaviors execute in correct order:
- [ ] Pre-processors run before handler
- [ ] Post-processors run after handler
- [ ] StateTransactionBehavior clones state before action
- [ ] RenderSubscriptionsPostProcessor triggers re-renders

**Tested by:** `event-stream-page-tests.cs` (validates pipeline logging)

#### 2. State Transaction Rollback
Test that state is restored on exception:
- [ ] Exception in handler rolls back state
- [ ] State GUID remains unchanged after exception
- [ ] ExceptionNotification is published

**Tested by:** `throw-exception-page-tests.cs`

#### 3. Request/Response Flow
Test basic mediator send/receive:
- [ ] Actions are dispatched correctly
- [ ] Handlers receive and process actions
- [ ] State is updated after successful action

**Tested by:** `counter-page-test.cs`

#### 4. Notification Publishing
Test notification system:
- [ ] Notifications are published
- [ ] Notification handlers receive notifications
- [ ] Multiple handlers can subscribe to same notification

**Tested by:** `event-stream-page-tests.cs` (pre/post pipeline notifications)

#### 5. Pre-Processor Functionality
Test pre-processor behavior:
- [ ] StateInitializationPreProcessor waits for state initialization
- [ ] PrePipelineNotificationRequestPreProcessor publishes notifications

**Tested by:** Client integration tests

#### 6. Post-Processor Functionality
Test post-processor behavior:
- [ ] RenderSubscriptionsPostProcessor triggers component re-renders
- [ ] PersistentStatePostProcessor saves state to storage

**Tested by:** `persistence-test-page-tests.cs`

#### 7. State Persistence
Test persistence functionality:
- [ ] State persists to session storage
- [ ] State persists to local storage
- [ ] State loads correctly on page reload
- [ ] State loads correctly in new tab

**Tested by:** `persistence-test-page-tests.cs`

#### 8. Routing Actions
Test routing state management:
- [ ] ChangeRouteActionSet works
- [ ] GoBackActionSet works
- [ ] PushRouteInfoActionSet works

**Tested by:** `change-route-page-tests.cs`, `go-back-page-tests.cs`

#### 9. JavaScript Interop
Test JS interop with mediator:
- [ ] JsonRequestHandler processes JSON requests
- [ ] Redux DevTools integration works

**Tested by:** `javascript-interop-page-tests.cs`

#### 10. Source Generator Output
Test that source generators produce valid code:
- [ ] PersistenceStateSourceGenerator generates load handlers
- [ ] ActionSetMethodSourceGenerator generates methods
- [ ] Generated handlers are registered correctly

**Tested by:** Architecture tests, persistence tests

### Test Coverage Gaps to Address

#### Missing Coverage (Create Before Migration)
- [ ] **Unit test for IPipelineBehavior signature** - Test that behaviors receive correct parameters
- [ ] **Unit test for MessagePreProcessor** - Test pre-processor abstract class
- [ ] **Unit test for MessagePostProcessor** - Test post-processor abstract class
- [ ] **Integration test for pipeline order** - Verify exact execution order
- [ ] **Unit test for ValueTask handling** - Ensure ValueTask is handled correctly

### Post-Migration Verification Checklist

After each migration task, run:
- [ ] `dotnet build` - Compilation succeeds
- [ ] `./scripts/test.cs` - All unit/integration tests pass
- [ ] `./scripts/e2e.cs` - All E2E tests pass

### Final Verification

- [ ] Compare test results to baseline - same pass/fail counts
- [ ] No new test failures introduced
- [ ] No performance regressions (E2E tests complete in similar time)
- [ ] Redux DevTools still works (manual verification)
- [ ] Sample applications work correctly

## Notes

### Test Project Overview

| Test Project | Type | Tests Mediator | Focus |
|--------------|------|----------------|-------|
| `timewarp-state-analyzer-tests` | Unit | Indirect | Analyzer rules |
| `timewarp-state-tests` | Unit | Yes | Core state functionality |
| `timewarp-state-plus-tests` | Unit | Yes | Plus features (routing, persistence) |
| `client-integration-tests` | Integration | Yes | Client-side state management |
| `test-app-architecture-tests` | Architecture | Indirect | Code conventions |
| `test-app-end-to-end-tests` | E2E | Yes | Full application flow |

### Commands Reference

```bash
# Run all unit/integration tests
./scripts/test.cs

# Run E2E tests
./scripts/e2e.cs

# Build solution
dotnet build

# Build NuGet packages
./scripts/package.cs

# Run specific test project
dotnet fixie timewarp-state-tests
```

### Known Test Issues

Document any pre-existing test failures here before migration:
- [ ] List any flaky tests
- [ ] List any skipped tests
- [ ] List any known failures with reasons

## Implementation Notes

### Baseline Test Run Results

**Date:** ___________

**Unit Tests:**
- timewarp-state-analyzer-tests: ___ passed / ___ failed
- timewarp-state-tests: ___ passed / ___ failed
- timewarp-state-plus-tests: ___ passed / ___ failed
- client-integration-tests: ___ passed / ___ failed
- test-app-architecture-tests: ___ passed / ___ failed

**E2E Tests:**
- test-app-end-to-end-tests: ___ passed / ___ failed

**Total:** ___ passed / ___ failed

**Known Pre-existing Issues:**
- (Document any)
