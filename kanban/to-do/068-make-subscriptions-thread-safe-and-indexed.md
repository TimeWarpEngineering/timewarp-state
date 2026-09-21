# Make Subscriptions thread-safe and indexed

## Description

Code review 2026-06-11, findings 13 and 20. Still true after 080. **064** made timer `Elapsed` real — that callback can race `Dispose` on Blazor Server.

`source/timewarp-state/subscriptions.cs`: plain `List<Subscription>`, no lock.

- `Add`: `Any()` over the whole list per `GetState` / subscribe
- `ReRenderSubscribers`: `Where().ToList()` over **all** subscriptions per action, then `Remove` dead refs
- `Remove`: `RemoveAll` by component id

`ReRenderSubscribers` runs on the mediator thread; `Dispose` → `Remove` on the renderer. WASM is usually single-threaded; **Blazor Server** is the exposure.

## Requirements

- Keyed index: state type → component id (e.g. `Dictionary<Type, Dictionary<string, Subscription>>`) plus a reverse index (or per-component list) so `Remove(component)` is cheap.
- **`lock`** around mutations and snapshot (per-circuit; contention is low). `ReRender()` **outside** the lock.
- `Add` = TryAdd / O(1). `ReRenderSubscribers(stateType)` iterates only that state’s snapshot.
- Dead `WeakReference`: still drop the subscription (current line-106 path).
- Tests: (1) `ReRenderSubscribers` racing `Remove` does not throw; (2) dead-ref cleanup still works.
- Keep public `Add` / `Remove` / `ReRenderSubscribers` signatures.

## Out of scope

- `Equals` / `GetHashCode` on `Subscriptions` unless they break
- ReaderWriterLockSlim (plain `lock` is enough)
- WASM-only “no lock” special case

## Checklist

- [ ] Indexed + locked Add/Remove/RemoveAll/ReRenderSubscribers
- [ ] Snapshot under lock; ReRender outside
- [ ] Concurrency test
- [ ] Dead-ref cleanup test
- [ ] `dev test` green

## Session

- Created: code review 2026-06-11
- 2026-09-21: cockpit — still valid after 064 timers; dispatch implementer-grok
