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

- [x] Indexed + locked Add/Remove/RemoveAll/ReRenderSubscribers
- [x] Snapshot under lock; ReRender outside
- [x] Concurrency test
- [x] Dead-ref cleanup test
- [x] `dev test` green
- [x] Review round 1 (general, effort 1); disposition clean

## Session

- Created: code review 2026-06-11
- 2026-09-21: cockpit — still valid after 064 timers; dispatch implementer-grok
- Implementer: grok session (2026-09-21)
- Review: grok oracle `01a0c42d-ddd2-7c92-b95c-31b07c3e9152` (2026-09-21) — effort 1 general; round 1; disposition clean

## Results

`Subscriptions` is no longer a plain unlocked `List`. Per-circuit mutations and snapshots take a `lock`; `ShouldReRender` / `ReRender` run outside it so a Blazor Server renderer `Dispose` cannot race or deadlock a mediator `ReRenderSubscribers` (finding 13). Lookups are keyed (finding 20).

**What landed**

- Forward index: state type → component id → `Subscription` (`TryAdd`, O(1)).
- Reverse index: component id → state types, so `Remove(component)` does not scan every subscription.
- `ReRenderSubscribers(stateType)` copies that state's values under the lock, then re-renders the snapshot. Dead `WeakReference` targets are dropped after the loop (same path as the old `List.Remove`).
- Public `Add` / `Remove` / `ReRenderSubscribers` signatures unchanged. `Equals` / `GetHashCode` retargeted at the new indexes so they still compile (reference equality, as with the old list).

**Files**

- `source/timewarp-state/subscriptions.cs`
- `tests/timewarp-state-tests/subscriptions/subscriptions-tests.cs` (new)

**Decisions**

- Plain `lock` on an `object` sync root, not `ReaderWriterLockSlim` and not a WASM-only unlock.
- Dead-ref removal is skipped if the same component id was replaced by a live subscriber between snapshot and cleanup.
- Duplicate `Add` for the same state type + component id is still a no-op (`TryAdd`).

**Review**

- Effort 1; roster: general; rounds: 1
- Final counts: bug 0/0/0 open/fixed/wontfix; suggestion 0; nit 0
- **Disposition: clean** (no issues raised; no fix loop)
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`

**Tests**

`dotnet run --file ./scripts/test.cs` exit 0:

- analyzer 19 passed
- source generator 4 passed
- state 26 passed, 1 skipped (includes `ReRenderSubscribers_Racing_Remove_Does_Not_Throw` and `ReRenderSubscribers_Drops_Dead_WeakReferences`)
- plus 25 passed, 1 skipped
- client integration 42 passed, 1 skipped (existing `Subscriptions_Should` suite still green)
- architecture 7 passed, 1 skipped

### How to validate

**Smoke**

```bash
dotnet tool restore
dotnet fixie timewarp-state-tests
```

**Expect**

- Exit 0.
- `SubscriptionsTests.Should_.ReRenderSubscribers_Racing_Remove_Does_Not_Throw` passes (200 subscribers, 1000 re-render loops racing `Remove`; no throw).
- `SubscriptionsTests.Should_.ReRenderSubscribers_Drops_Dead_WeakReferences` passes: after GC, `ReRenderSubscribers` drops the dead entry so a new component with the same id is `Add`ed and re-renders once.

**Automated gate**

```bash
dotnet run --file ./scripts/test.cs
# expect: exit 0; analyzer 19, generator 4, state 26+1 skipped, plus 25+1 skipped,
# client integration 42+1 skipped, architecture 7+1 skipped
```

**Not in scope:** ReaderWriterLockSlim; WASM-only “no lock”; changing `Equals`/`GetHashCode` semantics beyond compiling against the new indexes.
