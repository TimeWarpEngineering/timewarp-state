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

- [ ] Decide minimal guard vs per-state serialization (record here)
- [ ] Implement in `source/timewarp-state/features/pipeline/state-transaction-behavior.cs` (`:101`, `:131`)
- [ ] Log when a concurrent action advanced the state
- [ ] Interleaved-actions tests: A fails after B commits; B fails while A in flight
- [ ] Docs (transaction semantics)
- [ ] Code review

## Acceptance criteria

- In the interleaved test, B's committed change survives A's failure, and the reverse scenario is covered.
- No deadlock on single-threaded WASM (no blocking waits).
- Existing transaction/rollback tests green; `ganda repo audit` clean.

## Session

- Created: 2026-10-10 (Grok Bot, at Steven's request via Amina; not launched)

## Notes

- Source: 102 review-findings.md finding H2. Related batch: 109 (Medium findings).
