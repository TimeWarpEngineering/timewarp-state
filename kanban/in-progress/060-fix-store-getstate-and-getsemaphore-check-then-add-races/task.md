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

- [x] Rework `GetSemaphore` with `GetOrAdd`
- [x] Rework `GetState` with `GetOrAdd`; ensure `Initialize()`/`StateInitializedNotification` run once per state
- [x] Concurrency test: parallel first access to the same state type does not throw

## Session

- Implementer: grok session 01a0c4bf-20db-7d91-917c-55e575001e52 (2026-09-21)

## Results

Concurrent first `GetState`/`GetSemaphore` for a type no longer throws. The dictionary keeps one canonical instance; `Initialize()` and `StateInitializedNotification` run once until `Reset`/`RemoveState`. A losing `SemaphoreSlim` is disposed.

**What landed**

- `GetSemaphore`: value-form `GetOrAdd`; dispose the unused instance when another thread won; still `null` when the type is not in `States`
- `GetState`: per-type lock serializes first construction; `Initialize()` runs on the instance about to be inserted; `GetOrAdd` stores that instance; `StateInitializedNotification` publishes once
- `TryGetValue` hits are initialized because insert happens after `Initialize()`
- `RemoveState` takes the same per-type lock so it cannot interleave with first construction

**Files**

- `source/timewarp-state/store/store.cs`
- `source/timewarp-state/timewarp-state.csproj` (`InternalsVisibleTo` for `timewarp-state-tests`)
- `tests/timewarp-state-tests/store/store-get-or-add-tests.cs` (new)

**Decisions**

- Initialize under the per-type lock *before* `GetOrAdd`, so a dictionary hit is always initialized. A separate initialized-flag dictionary was rejected: `Reset`/`RemoveState` could clear the entry and leave the flag set, skipping `Initialize()` on the next instance.
- Losing `GetState` DI instances are disposed when they implement `IDisposable`.
- `IStore` signatures unchanged.

**Tests**

- `dotnet fixie timewarp-state-tests --tests '*StoreGetOrAdd*'`: 4 passed
- `dotnet fixie timewarp-state-tests`: 40 passed, 1 skipped
- `dotnet fixie client-integration-tests --tests '*StoreLifecycle*'`: 8 passed

### How to validate

**Smoke**

```bash
dotnet tool restore
dotnet fixie timewarp-state-tests --tests '*StoreGetOrAdd*'
```

**Expect**

- Exit 0, 4 passed.
- `Parallel_First_Access_To_Same_State_Does_Not_Throw_And_Initializes_Once`: 32 threads calling `GetState` for a new type return the same instance; `Initialize()` ran once; one `StateInitializedNotification`; `InitializedFlag` is 1 on every returned instance.
- `Parallel_GetSemaphore_Returns_Same_Instance_After_State_Exists`: `GetSemaphore` is null before first `GetState`; after `GetState`, 32 threads get the same `SemaphoreSlim` and do not throw.
- `GetState_After_Reset_Creates_New_Instance_And_Initializes_Again` and `GetState_After_RemoveState_Creates_New_Instance_And_Initializes_Again`: new instance, `Initialize()` count 2, two notifications.

**Automated gate**

```bash
dotnet fixie timewarp-state-tests
# expect: 40 passed, 1 skipped
dotnet fixie client-integration-tests --tests '*StoreLifecycle*'
# expect: 8 passed
dotnet run --file ./scripts/test.cs
# expect: exit 0; state suite includes the four StoreGetOrAdd cases
```

**Not in scope:** changing `IStore`; making `Reset` clear semaphores; `GetSemaphore` during `Reset` of a type that still has a leftover semaphore.
