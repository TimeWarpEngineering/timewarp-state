# Round 1 — general
**Date:** 2026-09-21
**Scope reviewed:** branch `task/060-fix-store-getstate-and-getsemaphore-check-then-add` vs `origin/master` (implement commit `e7775910`; kanban results `f2bf225f`); product files `store.cs`, `timewarp-state.csproj`, new `store-get-or-add-tests.cs`; surrounding `i-store.cs`, `state.cs`, `state-transaction-behavior.cs`, `store.redux-dev-tools.cs`, `store-lifecycle-tests.cs`

## Summary

The change stops `GetState`/`GetSemaphore` from throwing on concurrent first access: both use `ConcurrentDictionary.GetOrAdd`, and `GetState` serializes construction with a per-type lock so `Initialize()` runs on the instance that is then inserted, and `StateInitializedNotification` publishes once. A `TryGetValue` hit is therefore initialized; `RemoveState` takes the same lock; `IStore` is unchanged. The `GetState` GetOrAdd loser/dispose branch is not reachable under current writers (the lock covers construction and `RemoveState`; `Reset` only clears; `SetState` updates existing keys) and is harmless defensive cleanup. `dotnet fixie timewarp-state-tests --tests '*StoreGetOrAdd*'` reported 4 passed; 32-way first access, Reset/RemoveState re-init, and GetSemaphore-null-until-state-exists all match the brief. Risk is low; leftover semaphore across `Reset` remains out of scope.

## Issues

No issues found.
