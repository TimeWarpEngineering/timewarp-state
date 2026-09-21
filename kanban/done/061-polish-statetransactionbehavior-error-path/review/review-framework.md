# Review framework — task 061

**Date:** 2026-09-21
**Host task:** kanban/in-progress/061-polish-statetransactionbehavior-error-path/
**Diff scope:** branch `task/061-polish-statetransactionbehavior-error-path` vs `origin/master` (implement commit `9a420d99`; kanban results `93e16d2d`)
**Plan / brief:** Code review 2026-06-11 findings 2 (revised) and 27. Catch-rollback-notify stays. Publish `ExceptionNotification` with `CancellationToken.None` so a cancelled request token cannot skip reporting. `OperationCanceledException` still rolls back and is not published. Catch log says handler failure, not clone failure. Constructor already logs `StateTransactionBehavior`.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle `01a0c492-101a-7521-9705-9bdb3922c2f1` (2026-09-21); implementer grok `01a0c483-ea23-7351-9635-c575984f9fd3` (2026-09-21)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Product files in scope

- `source/timewarp-state/features/pipeline/state-transaction-behavior.cs`
- `tests/timewarp-state-tests/pipeline/state-transaction-behavior-tests.cs` (new)
- `tests/timewarp-state-tests/global-usings.cs`
- Surrounding call sites: `tests/client-integration-tests/pipeline/state-transaction-tests.cs`; `ExceptionNotification`; `IPublisher<ClientPipeline>`

## Requirements to check

- `Publisher.Publish(exceptionNotification, CancellationToken.None)` — not the request token
- OCE: rollback, do **not** publish `ExceptionNotification` (cancellation is not a failure)
- Other exceptions still notify even when the request token is cancelled
- Catch-block message is handler failure (`Error handling action`), not clone (`Error cloning State`)
- Constructor class-name log is `StateTransactionBehavior` (finding 27, already done)
- Test: cancelled token → rollback + notification handlers **do** run for non-OCE
- Test: OCE rolls back without notification
- Out of scope: changing clone strategy / ICloneable; removing rollback; rethrowing OCE to callers
