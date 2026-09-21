# Round 2 — merged findings
**Date:** 2026-09-21
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-source-generator/persistence-state-source-generator.cs (ClassModel / EquatableDiagnosticLocation); source/timewarp-state-source-generator/is-external-init.cs
- Description: `ClassModel` stored a Roslyn `Location`, pinning `SyntaxTree` and using reference equality.
- Suggestion: Equatable path+span in the model; reconstruct `Location.Create` in `Execute`; value-equal `ClassModel`.
- Source: general
- Disposition notes: Re-verified in round 2. Incremental model holds `EquatableDiagnosticLocation` only. TWSG001 still on `NestedWidgetState` identifier; skip-emit unchanged; generator tests 4 passed. No new findings.

## Duplicates / conflicts

None. Prior M1 carried forward as fixed. No new IDs.
