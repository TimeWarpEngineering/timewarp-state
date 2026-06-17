# Make Subscriptions thread-safe and indexed

## Description

Code review 2026-06-11, findings 13 (PLAUSIBLE race) and 20 (`code-review-2026-06-11.md`).

`source/timewarp-state/subscriptions.cs` holds a plain `List<Subscription>` (line 7) on a scoped service with no synchronization:

- **Race exposure (Blazor Server):** `ReRenderSubscribers` both reads (lines 87–89) and mutates (`TimeWarpStateComponentReferencesList.Remove(subscription)`, line 106) from the mediator pipeline thread, while component `Dispose` calls `Remove`/`RemoveAll` on the renderer dispatcher. An action dispatched off the circuit's sync context (timer, hub event, `Task.Run`) racing a component disposal can corrupt the list or throw.
- **Hot-path cost:** `Add` does a LINQ `Any()` linear scan per `GetState` call (line 26 — i.e., per component per render), and `ReRenderSubscribers` allocates `Where().ToList()` per dispatched action. O(total subscriptions) work on the two hottest paths.

## Fix

Restructure as a keyed index, e.g. `Dictionary<Type, Dictionary<string, Subscription>>` (state type → component id), with a lock around mutations and snapshots:

- `Add` becomes an O(1) TryAdd
- `ReRenderSubscribers(stateType)` iterates only that state's subscribers, snapshot taken under the lock
- `Remove(componentId)` needs a reverse index (or per-component subscription list) to stay cheap

One structure change addresses both the race and the scans.

## Checklist

- [ ] Choose structure + locking strategy (plain lock is fine; contention is per-circuit)
- [ ] Rework Add / Remove / RemoveAll / ReRenderSubscribers
- [ ] Concurrency test: ReRenderSubscribers racing component disposal does not throw
- [ ] Behavior test: dead-reference cleanup (current line 106 path) still works
