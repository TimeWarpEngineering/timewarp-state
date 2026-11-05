# Round 2 — merged findings
**Date:** 2026-11-06
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 3 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 1 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/action-set-shape.cs:63 (action-set-method-generator.cs:93)
- Description: TWSG003 fired on record and struct Actions, and on ActionSets in record or struct states, which the old generator ignored.
- Source: general (round 1)
- Disposition notes: Fixed in 8c4f08d9. `ActionSetShape.WasServedBefore` gates TWSG003 (`action-set-method-generator.cs:100`).
  It has 7 silent-skip tests. The round-1 scratch repros compile. Re-verified in round 2.

### M2 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/action-set-constructor-parser.cs:83
- Description: A class Action with primary and explicit constructors changed signature.
- Source: general (round 1)
- Disposition notes: Fixed in 8c4f08d9. A non-record class takes its first explicit constructor first, and the test is
  flipped. The scratch caller `Set("x")` compiles. Re-verified in round 2.

### M3 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/action-catalog-source-generator.cs:88
- Description: A cataloged record Action with a hand-written method of another signature broke the generated catalog.
- Source: general (round 1)
- Disposition notes: Fixed in 8c4f08d9. The catalog skips the entry unless `UserMethodCallable` is true
  (`action-catalog-source-generator.cs:94`). It has 2 new catalog tests. The scratch repro compiles. Re-verified in
  round 2.

### M4 — Severity: nit — Status: wontfix
- File: source/timewarp-state-source-generator/action-set-shape.cs:113
- Description: `IsCallableByCatalog` ignores `RefKind`. Take a `[CatalogAction]` record `Action(int Amount)` with a
  hand-written `Task Bump(ref int amount, CancellationToken? externalCancellationToken = null)`. The executor passes
  the argument by value and gets CS1620 (confirmed in a scratch project). Master compiled this because it never
  cataloged records.
- Suggestion: require `RefKind.None` or `RefKind.In` for every parameter.
- Source: general
- Disposition notes: wontfix, decided by the review oracle (orchestrator) in round 2. Hitting it takes three things at
  once:
  - a hand-written method that copies the generated signature exactly, including the `externalCancellationToken`
    parameter name;
  - a `ref` or `out` added to an Action parameter, which also rules out `async` (CS1988);
  - `[CatalogAction]` on that record.

  No such code is in the repo or the samples, and no realistic consumer writes it. The fix is one line. A later task
  can add it to `IsCallableByCatalog` and a test if anyone hits it. It is not worth another fix pass on this id.

## Duplicates / conflicts

- None. M4 is new in round 2. It is a residual edge of the M3 fix and has its own ID.
