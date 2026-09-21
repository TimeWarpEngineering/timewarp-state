# Round 1 — merged findings
**Date:** 2026-09-21
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

No issues raised.

## Duplicates / conflicts

None — single reviewer, zero issues.

## Merge notes

Re-verified against the repo: `GetState` double-checks under a per-type lock, calls `Initialize()` on the instance about to be inserted, then `GetOrAdd`; `StateInitializedNotification` publishes only on the inserted instance. `RemoveState` takes the same lock. `GetSemaphore` uses value-form `GetOrAdd` and disposes the unused `SemaphoreSlim`; returns null when the type is not in `States`. `IStore` is unchanged. `dotnet fixie timewarp-state-tests --tests '*StoreGetOrAdd*'`: 4 passed.
