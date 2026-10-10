# Round 1 — merged findings
**Date:** 2026-10-11
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 1 | 1 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-state/store/store.cs:85
- Description: `Reset` aborted on the first throwing `CancelOperations` and skipped keys present only in `PreviousStates` / `StateInitializationTasks`.
- Suggestion: Iterate the union of key sets; collect per-key failures and rethrow.
- Source: general
- Disposition notes: Fixed. `Reset` walks the distinct union of `States`, `PreviousStates` and `StateInitializationTasks` keys, continues past a failing key, and throws `AggregateException` afterwards. Design region updated.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-plus/features/timers/timer-state/timer-state.cs:26
- Description: `CircuitContext` capture depends on which thread first makes the Store construct `TimerState`; undocumented.
- Suggestion: Document it, or capture deterministically.
- Source: general
- Disposition notes: Fixed by documentation in the Design region (capture happens at first construction; touch `TimerState` from the circuit to get marshalling). Deterministic capture would need a circuit-scoped service and is beyond M8's scope.

### M3 — Severity: nit — Status: fixed
- File: source/timewarp-state-analyzer/state-read-only-public-properties-analyzer.cs:63
- Description: Partial states ran the symbol loop once per declaration, so each property could be reported once per partial declaration. Indexers with public setters are now reported.
- Suggestion: Report only from the declaration that holds the property; decide indexer behaviour.
- Source: general
- Disposition notes: Fixed the duplicate: the location is now the property location inside the current `TypeDeclarationSyntax`. New test `Given_PartialTimeWarpState_ReportsOnce`. Indexers stay reported on purpose: a public indexer setter is public mutable state, which is what TWS0012 forbids.

### M4 — Severity: nit — Status: wontfix
- File: source/timewarp-state/features/pipeline/state-transaction-behavior.cs:139
- Description: Rollback check `Store.GetState` can re-create a state removed by `Reset`/`RemoveState` mid-action.
- Suggestion: Optional non-creating lookup.
- Source: general
- Disposition notes: wontfix (orchestrator). The reviewer rated it acceptable. The re-created state is what the next `GetState` would build anyway, the `ReferenceEquals` check then skips rollback correctly, and adding a non-creating lookup would widen `IStore` again right after M5 narrowed it.

## Duplicates / conflicts

- None.
