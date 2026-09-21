# Review framework — task 068

**Date:** 2026-09-21
**Host task:** kanban/in-progress/068-make-subscriptions-thread-safe-and-indexed/
**Diff scope:** branch `task/068-make-subscriptions-thread-safe-and-indexed` vs `origin/master` (implement commit `f59c23a7`; kanban results `be6e1a68`)
**Plan / brief:** Code review 2026-06-11 findings 13 (unsynchronized list races on Blazor Server) and 20 (O(n) subscription scans). Replace `List<Subscription>` with keyed indexes plus a plain `lock`. Snapshot under the lock; `ShouldReRender` / `ReRender` outside it. Keep public `Add` / `Remove` / `ReRenderSubscribers` signatures. Tests: race `ReRenderSubscribers` vs `Remove` does not throw; dead `WeakReference` cleanup still works.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle `01a0c42d-ddd2-7c92-b95c-31b07c3e9152` (2026-09-21)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Product files in scope

- `source/timewarp-state/subscriptions.cs`
- `tests/timewarp-state-tests/subscriptions/subscriptions-tests.cs` (new)
- Surrounding call sites: `source/timewarp-state/components/timewarp-state-component.cs` (`GetState` Add, `Dispose` Remove), `source/timewarp-state/components/timewarp-state-input-component.cs` (same), `source/timewarp-state/features/render-subscriptions/render-subscriptions-post-processor.cs` (`ReRenderSubscribers`)
- Existing coverage: `tests/client-integration-tests/subscriptions/subscriptions-tests.cs`

## Requirements to check

- Keyed index: state type → component id → `Subscription` (e.g. `Dictionary<Type, Dictionary<string, Subscription>>`)
- Reverse index (or per-component list) so `Remove(component)` is cheap
- `lock` around mutations and snapshot (per-circuit; plain `lock`, not `ReaderWriterLockSlim`)
- `ReRender()` / `ShouldReRender` **outside** the lock (no deadlock with renderer `Dispose` → `Remove`)
- `Add` = TryAdd / O(1); duplicate state type + component id is a no-op
- `ReRenderSubscribers(stateType)` iterates only that state's snapshot
- Dead `WeakReference`: still drop the subscription
- Tests: (1) `ReRenderSubscribers` racing `Remove` does not throw; (2) dead-ref cleanup still works
- Public `Add` / `Remove` / `ReRenderSubscribers` signatures unchanged
- Out of scope: `Equals` / `GetHashCode` semantics unless they break; WASM-only “no lock”; `ReaderWriterLockSlim`
