# Round 1 — general
**Date:** 2026-10-11
**Scope reviewed:** branch task/109-... vs origin/master (0d0c7f87)

## Summary
Read the diff for M2-M13 against the code (store, transaction behavior, pre-processor, timer, push-route-info, persistence, analyzers, csproj/AOT suppressions, event ids, verify-samples). The checklist claims match the code, no stale references to the old analyzer ids, `GetSemaphore`, or `PreRender`/`Server` remain outside archived docs, and tests exist for M3, M4, M7, M8 and the analyzer changes. No blocking defects found. A few low-severity robustness points are below. Build and tests were not run.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-state/store/store.cs:85
- Description: `Reset` loops `RemoveStateByName` over `States.Keys`. If one state's `CancelOperations()` throws, the loop aborts and the remaining states stay in the store, which is a partial reset again. Keys that exist only in `PreviousStates` or `StateInitializationTasks` (state already gone from `States`) are also not cleared.
- Suggestion: Iterate the union of the key sets, and either catch/log per key and rethrow an aggregate after the loop, or document that `CancelOperations` must not throw.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-state-plus/features/timers/timer-state/timer-state.cs:26
- Description: `CircuitContext = SynchronizationContext.Current` is captured at construction. `Store.GetState` constructs the state lazily inside its lock on whichever thread first touches it, so a first access from a thread-pool continuation (`ConfigureAwait(false)`, a timer callback) captures null. The M8 marshalling guarantee then silently does not apply. The Design comment covers the null case, but not that capture is first-access dependent.
- Suggestion: Say so in the Design region and the docs, or capture the context somewhere that is deterministic per circuit (for example from an injected service created in the circuit).
- Status: open

### Issue 3 — Severity: nit
- File: source/timewarp-state-analyzer/state-read-only-public-properties-analyzer.cs:63
- Description: The symbol-based loop treats indexers as properties. A public indexer with a public `set` on a State is now reported as TWS0012 with the name `this[]`. The old syntax-based rule never visited indexers. Partial states also run the loop once per declaration node, so each property is reported repeatedly (Roslyn dedups identical diagnostics, so likely harmless).
- Suggestion: Skip `property.IsIndexer`, or add a test that pins the intended behaviour. Optionally report only for the declaring syntax reference that belongs to `context.Node`.
- Status: open

### Issue 4 — Severity: nit
- File: source/timewarp-state/features/pipeline/state-transaction-behavior.cs:139
- Description: The rollback check calls `Store.GetState(enclosingStateType)`, which creates and initializes a fresh state (and publishes `StateInitializedNotification`) if `Store.Reset`/`RemoveState` ran during the failed or cancelled action. With M4, `Reset` now really removes the in-flight state, so this path is reachable (for example the test-app `ResetStore` action failing after `Reset`).
- Suggestion: Acceptable as is. Optionally add a non-creating lookup so a rollback after Reset does not resurrect the state.
- Status: open
