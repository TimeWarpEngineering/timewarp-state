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
- [ ] Publish with CancellationToken.None
- [ ] OCE: rollback, skip notification
- [ ] Catch log text
- [ ] Unit test cancelled token / OCE vs other exceptions

## Session

- Created: code review 2026-06-11
- 2026-09-21: cockpit shrunk after 062 merge. Next after this: 060.
