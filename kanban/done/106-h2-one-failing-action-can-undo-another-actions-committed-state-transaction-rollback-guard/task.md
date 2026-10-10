# Task 106: H2: One failing action can undo another action's committed state (transaction rollback guard)

## Description

When two actions overlap on the same state, a failure in one restores the original state and silently
discards the other action's committed changes (`state-transaction-behavior.cs`). Second in Fable's order.

Filed 2026-10-10 at Steven's request (relayed by Amina) from Claude Fable's full codebase review,
task 102 (`kanban/done/102-full-codebase-review-by-claude-fable-review-only/`, PR #629, `00ade373`).
Not launched.

## Finding H2 (Fable review)

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 34-39 (PR #629, merged as `00ade373`):

> ### H2. Overlapping actions on one state: a failure in one rolls back the other's committed work
>
> - `source/timewarp-state/features/pipeline/state-transaction-behavior.cs:101` installs the clone as the live state before `next`, and `:131` restores `originalState` on failure unconditionally. There is no per-state serialization: `IStore.GetSemaphore` exists but the behavior does not use it.
> - Scenario (Blazor Server or WASM, both single-scope): action A on `CounterState` starts, clone `c1` becomes live, A awaits an HTTP call. Action B on the same state starts, clones `c1` into `c2`, increments, completes, renders. A then throws. The catch restores `originalState`, discarding B's completed increment. The reverse also holds: if B fails, `c1` is restored and A's later writes land on the orphaned `c2` and are lost. In both cases nothing is logged about the second action.
> - Handlers already re-read `Store.GetState<T>()` per access, so the ordinary interleaving works. It is only the rollback that is wrong, and it is wrong exactly when the user most expects the store to be consistent.
> - Suggestion (minimal): roll back only if the store still holds this action's clone: `if (ReferenceEquals(Store.GetState(enclosingStateType), newState)) Store.SetState(originalState); else log that a concurrent action advanced the state`. Suggestion (full): serialize actions per state with a non-blocking `SemaphoreSlim.WaitAsync` in the transaction behavior (safe on single-threaded WASM because it never blocks), and delete `IStore.GetSemaphore`. Add a unit test with two interleaved `next` delegates to pin the behavior either way.

## Order

Fable's recommended order (verbatim, `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md`, Overall assessment):

> Recommended order of attack: the package dependency (one line), the transaction rollback guard (one `ReferenceEquals` check), a `[CloneShared]`-style attribute or equivalent for injected members, then the DevTools and persistence dead paths, then `IsAotCompatible` on the two remaining runtime packages.

Filed tasks in that order: 105 (H1, package dependency) -> 106 (H2, rollback guard) -> 107 (H3, share-by-reference attribute) -> 104 and 109 M1/M9 (dead DevTools and persistence paths) -> 109 M2 (AOT checks on Blazor and Plus). 108 (H4) and the remaining Medium items are not in Fable's ordered list; schedule them after 107.

## Requirements

- Rollback must never discard another action's committed work: at minimum the `ReferenceEquals` guard
  (roll back only if the store still holds this action's clone; otherwise log that a concurrent action advanced
  the state), or per-state serialization with non-blocking `SemaphoreSlim.WaitAsync` (then `IStore.GetSemaphore`
  can go; see 109 M5).
- A unit test with two interleaved `next` delegates pins the chosen behavior.

## Checklist

- [x] Decide minimal guard vs per-state serialization (record here)
- [x] Implement in `source/timewarp-state/features/pipeline/state-transaction-behavior.cs` (`:101`, `:131`)
- [x] Log when a concurrent action advanced the state
- [x] Interleaved-actions tests: A fails after B commits; B fails while A in flight
- [x] Docs (transaction semantics)
- [x] Code review

Decision: minimal `ReferenceEquals` guard, not per-state serialization. Fable's order is one reference check. A per-state `SemaphoreSlim` can deadlock when a handler waits on work that re-enters the same state; `WaitAsync` does not remove that deadlock. `IStore.GetSemaphore` stays for task 109 M5.

Rollback restores the snapshot only when the store holds this action's clone. A different instance is a later action's committed clone: the behavior leaves it in place and logs warning `StateTransactionBehavior_ConcurrentAdvance` (event id 405): "Skipping rollback because a concurrent action advanced the state." In-place writes on the live clone are not a separate commit; the action that installed that clone still rolls them back.

## Acceptance criteria

- In the interleaved test, B's committed change survives A's failure, and the reverse scenario is covered.
- No deadlock on single-threaded WASM (no blocking waits).
- Existing transaction/rollback tests green; `ganda repo audit` clean.

## Session

- Created: 2026-10-10 (Grok Bot, at Steven's request via Amina; not launched)
- Implementation: Grok session 01a126c3-5c43-7a40-854c-f8cfd7fa93d0 (2026-10-11)
- Review: Claude Opus 5.5 review oracle (2026-10-11), effort 2, roster general
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 120 — 2026-10-10T17:19:12Z

## Results

`StateTransactionBehavior` rolls back on failure only when the store holds the clone that action installed. A newer clone from an overlapping action stays in the store, and the skip is logged. Single-action rollback is unchanged. Actions are not serialized, and the behavior does not take a blocking wait.

### Files

- `source/timewarp-state/features/pipeline/state-transaction-behavior.cs`
- `source/timewarp-state/event-ids.cs` (event id 405 `StateTransactionBehavior_ConcurrentAdvance`)
- `tests/timewarp-state-tests/pipeline/state-transaction-behavior-tests.cs`
- `documentation/overview.md`
- `documentation/topics/cloning.md` (Transaction rollback)

### Decisions

Minimal `ReferenceEquals` guard. Per-state serialization is deferred because a lock around `Handle` deadlocks a re-entrant wait on the same state. `GetSemaphore` is unchanged (109 M5).

The guard treats "advanced" as a different state instance. An in-flight handler that only mutates the live clone does not count as a commit. `Restore_Earlier_Clone_When_Overlapping_Action_Fails_While_Earlier_Action_Is_In_Flight` pins that case: the overlapping failure restores the earlier clone, and the earlier action's later re-read write sticks.

Code review is the host review node. This implement pass did not run it.

### Review disposition

- Rounds: 1. Effort 2, roster: general.
- Final counts: bug 0. Suggestion 1 (fixed). Nit 1 (wontfix). Open 0.
- Disposition: **accepted-exceptions**. M1 (docs did not say that a failing action's in-place writes, made before a concurrent action cloned the state, carry into that clone) is fixed in `documentation/topics/cloning.md`. M2 (the skip warning says "concurrent action advanced" after a `RemoveState` or `Reset`) is wontfix. The skip is correct, and the wording matches the recorded decision and the tests.
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

### Test outcomes

`./bin/dev test` exited 0 (2026-10-11):

| Suite | Result |
| --- | --- |
| analyzer | 38 passed |
| source generator | 75 passed |
| state | 104 passed, 1 skipped |
| plus | 32 passed, 1 skipped |
| telemetry | 13 passed |
| client integration | 65 passed, 1 skipped |
| architecture | 7 passed, 1 skipped |

`StateTransactionBehaviorTests.Should_` is 9 passed, including the three overlap tests. `ganda repo audit` exited 0: 29 passed, 1 non-blocking advisory (`kebab-path-names` on existing Blazor `lib.module` paths).

### How to validate

**Smoke**

```bash
dotnet fixie timewarp-state-tests --tests '*StateTransactionBehavior*'
```

**Expect**

9 passed. `Keep_Later_Commit_When_Earlier_Action_Fails_After_Later_Action_Commits` leaves value 42 on the later action's clone. `Keep_Successor_Commit_When_Overlapping_Action_Fails_While_Earlier_Action_Is_In_Flight` leaves value 77 on the successor clone and records one `StateTransactionBehavior_ConcurrentAdvance` warning. `Restore_Earlier_Clone_When_Overlapping_Action_Fails_While_Earlier_Action_Is_In_Flight` restores the earlier clone (value 3, not the overlapping write 9); after that action resumes, the same instance is 11.

**Automated gate**

```bash
./bin/dev test
ganda repo audit
```

**Not in scope:** deleting `IStore.GetSemaphore` and per-state serialization.

## Notes

- Source: 102 review-findings.md finding H2. Related batch: 109 (Medium findings).
