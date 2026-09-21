# Polish StateTransactionBehavior error path

## Description

Code review 2026-06-11, findings 2 (revised) and 27. Catch-rollback-notify stays.

**Already done:** constructor log uses `StateTransactionBehavior`, not `ReduxDevToolsBehavior`.

**Still open:**

1. `Publisher.Publish(exceptionNotification, cancellationToken)` — if the handler failed because that token was cancelled, Publish throws OCE and `ExceptionNotification` never runs.
2. `OperationCanceledException` is reported as an error. Decision: **rollback yes**; **skip ExceptionNotification for OCE** (cancellation is not a failure). Other exceptions still notify.
3. Catch log still says `"Error cloning State"` for a **handler** failure (clone is outside the try).

## Requirements

- Publish with `CancellationToken.None`
- OCE: rollback, do **not** publish ExceptionNotification
- Fix catch-block message (handler failure, not clone)
- Test: cancelled token → rollback + notification handlers **do** run for non-OCE; OCE rolls back without notification

## Out of scope

- Changing clone strategy / ICloneable
- Removing rollback

## Checklist

- [x] Constructor class-name log (already StateTransactionBehavior)
- [x] Publish with CancellationToken.None
- [x] OCE: rollback, skip notification
- [x] Catch log text
- [x] Unit test cancelled token / OCE vs other exceptions

## Session

- Created: code review 2026-06-11
- 2026-09-21: cockpit shrunk after 062 merge. Next after this: 060.
- Implementer: grok session 01a0c483-ea23-7351-9635-c575984f9fd3 (2026-09-21)

## Results

Handler failures still roll back to the pre-action clone. `ExceptionNotification` is published with `CancellationToken.None`, so a cancelled request token cannot skip error reporting. `OperationCanceledException` still rolls back and is not published — cancellation is not a failure. The catch log says handler failure, not clone failure.

**What landed**

- `Publisher.Publish(exceptionNotification, CancellationToken.None)`
- Skip warning log and `ExceptionNotification` when the handler throws `OperationCanceledException`
- Catch message: `Error handling action. Type:{enclosingStateType}`
- Recording publisher in unit tests throws if given a cancelled token, so a regression to the request token fails the test

**Files**

- `source/timewarp-state/features/pipeline/state-transaction-behavior.cs`
- `tests/timewarp-state-tests/pipeline/state-transaction-behavior-tests.cs` (new)
- `tests/timewarp-state-tests/global-usings.cs`

**Decisions**

- Catch-rollback-notify stays. Clone strategy unchanged.
- One `catch (Exception)` with an `is OperationCanceledException` branch: rollback for both; notify only for non-OCE.
- `TaskCanceledException` is skipped as well (it is OCE).

**Tests**

`dotnet run --file ./scripts/test.cs` exit 0:

- analyzer 19 passed
- source generator 4 passed
- state 36 passed, 1 skipped (includes three `StateTransactionBehaviorTests.Should_` cases)
- plus 25 passed, 1 skipped
- client integration 42 passed, 1 skipped
- architecture 7 passed, 1 skipped

`dotnet fixie client-integration-tests --tests '*StateTransaction*'`: 5 passed (clone, preserve, rollback, notify, continue-after-exception).

### How to validate

**Smoke**

```bash
dotnet tool restore
dotnet fixie timewarp-state-tests --tests '*StateTransactionBehavior*'
```

**Expect**

- Exit 0, 3 passed.
- `Rollback_And_Publish_Notification_When_Handler_Throws_On_Cancelled_Token`: cloned state is discarded; original value `5` is restored; `ExceptionNotification` is published with `CancellationToken.None` even though the request token is cancelled.
- `Rollback_Without_Notification_When_Handler_Throws_OperationCanceledException`: original value `5` is restored; publications list is empty.
- `Rollback_Without_Notification_When_OperationCanceledException_Uses_Cancelled_Token`: same rollback-without-notify when the OCE carries the cancelled request token.
- Catch log in `state-transaction-behavior.cs` is `Error handling action`, not `Error cloning State`.
- `Publisher.Publish` in that catch uses `CancellationToken.None`.

**Automated gate**

```bash
dotnet run --file ./scripts/test.cs
# expect: exit 0; analyzer 19, generator 4, state 36+1 skipped, plus 25+1 skipped,
# client integration 42+1 skipped, architecture 7+1 skipped
dotnet fixie client-integration-tests --tests '*StateTransaction*'
# expect: exit 0; 5 passed
```

**Not in scope:** clone strategy / ICloneable; removing rollback; rethrowing OCE to callers.
