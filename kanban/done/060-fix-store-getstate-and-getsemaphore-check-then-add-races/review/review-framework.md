# Review framework — task 060

**Date:** 2026-09-21
**Host task:** kanban/in-progress/060-fix-store-getstate-and-getsemaphore-check-then-add-races/
**Diff scope:** branch `task/060-fix-store-getstate-and-getsemaphore-check-then-add` vs `origin/master` (implement commit `e7775910`; kanban results `f2bf225f`)
**Plan / brief:** Code review 2026-06-11 finding 7. `GetState` and `GetSemaphore` used TryGetValue → create → TryAdd → throw on loser. Concurrent first access threw; losing `SemaphoreSlim` leaked. Rework with `ConcurrentDictionary.GetOrAdd`; `Initialize()` and `StateInitializedNotification` once per canonical instance until Reset/RemoveState.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle `01a0c4d4-9c79-7e91-9da4-4138747e5944` (2026-09-21); implementer grok `01a0c4bf-20db-7d91-917c-55e575001e52` (2026-09-21)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Product files in scope

- `source/timewarp-state/store/store.cs`
- `source/timewarp-state/timewarp-state.csproj` (`InternalsVisibleTo` for `timewarp-state-tests`)
- `tests/timewarp-state-tests/store/store-get-or-add-tests.cs` (new)

Surrounding call sites (not modified, still in review scope for regressions):

- `source/timewarp-state/store/i-store.cs`
- `source/timewarp-state/state/state.cs` (`IDisposable`, `Initialize`, `CancelOperations`)
- `source/timewarp-state/features/pipeline/state-transaction-behavior.cs` (`GetState`)
- `source/timewarp-state/store/store.redux-dev-tools.cs` (`GetState`)
- `tests/client-integration-tests/store/store-lifecycle-tests.cs`

## Requirements to check

- Concurrent first `GetState` for a type does not throw; dictionary keeps one canonical instance
- `Initialize()` and `StateInitializedNotification` run once until `Reset`/`RemoveState`
- A `TryGetValue` hit is initialized (insert happens after `Initialize()`)
- `GetSemaphore` uses GetOrAdd; unused loser `SemaphoreSlim` is disposed; still `null` when the type is not in `States`
- `RemoveState` cannot interleave with first construction (same per-type lock)
- `IStore` signatures unchanged
- Test: parallel first access to the same state type does not throw

## Out of scope

- Changing `IStore`
- Making `Reset` clear semaphores
- `GetSemaphore` during `Reset` of a type that still has a leftover semaphore
