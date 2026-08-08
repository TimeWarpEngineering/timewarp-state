# Fix Store GetState and GetSemaphore check-then-add races

## Description

Code review 2026-06-11, finding 7 (`code-review-2026-06-11.md`).

Both methods in `source/timewarp-state/store/store.cs` use a non-atomic `TryGetValue` → create → `TryAdd` → **throw on TryAdd failure** pattern:

- `GetState` (~lines 125–138): two concurrent first accesses for the same state type both construct and `Initialize()` a state; the `TryAdd` loser throws `InvalidOperationException("An element with the key ... already exists")`, faulting that request.
- `GetSemaphore` (~lines 96–103): same race — and the throw happens exactly in the high-concurrency case the semaphore exists to serialize; the losing `SemaphoreSlim` also leaks undisposed.

The Store is scoped (per-circuit in Blazor Server), but concurrent access within a circuit is reachable via thread-pool continuations, timers, and background dispatches.

## Fix

Use `ConcurrentDictionary.GetOrAdd` and tolerate the losing instance instead of throwing:

- `GetSemaphore`: `Semaphores.GetOrAdd(typeName, _ => new SemaphoreSlim(1, 1))` (a loser SemaphoreSlim with no waiters is harmless, or dispose it explicitly after losing the race).
- `GetState`: `GetOrAdd` with the create logic; decide whether `Initialize()` runs only on the winning instance (preferred — initialize the canonical instance after `GetOrAdd` returns, guarded to run once), since `Initialize` can have side effects and `StateInitializedNotification` should publish once.

## Checklist

- [ ] Rework `GetSemaphore` with `GetOrAdd`
- [ ] Rework `GetState` with `GetOrAdd`; ensure `Initialize()`/`StateInitializedNotification` run once per state
- [ ] Concurrency test: parallel first access to the same state type does not throw
