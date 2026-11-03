# Round 2 — merged findings
**Date:** 2026-11-04
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:2088
- Description: `CanName` accepted `private protected` types in this assembly, so generated code could reference a name that does not compile (CS0122).
- Source: general (round 1)
- Disposition notes: Round 2 re-verified this as fixed. `ProtectedAndInternal` is unnameable in every assembly. Regressions `Given_Private_Protected_ICloneable_State` and `Given_GenerateClone_Member_Of_Private_Protected_Type` pass, and both assert no compiler errors.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1488
- Description: The comparer constructor path passed `source.Count` to every `int` parameter.
- Source: general (round 1)
- Disposition notes: Round 2 re-verified this as fixed. `IsCapacityParameter` controls eligibility, scoring, and argument mapping. `Given_Non_Capacity_Int_Is_Not_Passed_As_Count` passes.

## Duplicates / conflicts

- None. This round raised no new findings.
