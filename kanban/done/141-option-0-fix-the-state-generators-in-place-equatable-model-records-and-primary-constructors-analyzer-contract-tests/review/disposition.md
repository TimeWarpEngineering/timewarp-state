# Disposition — task 141

**Date:** 2026-11-06
**Outcome:** accepted-exceptions
**Rounds:** 2
**Final open count:** 0

## Summary

Round 1 (effort 3, general) raised three bugs: M1, M2 and M3. Each one broke "not breaking" for a shape the old
generators ignored or served differently. Fix pass 1 (8c4f08d9) fixed all three.

Round 2 (effort 2, general) re-verified them in three ways:
- the generator suite: 150 passed;
- a test-app build: 0 errors;
- a scratch consumer, where every round-1 repro now compiles.

Round 2 also raised one new nit, M4, against the M3 fix. It is accepted as wontfix, so the outcome is
accepted-exceptions with 0 open.

## Exception log (if accepted-exceptions)

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M4 | nit | `IsCallableByCatalog` ignores `RefKind`. It fails only for a `[CatalogAction]` record whose hand-written method copies the generated signature, including `externalCancellationToken`, and then adds `ref`/`out`, so that method cannot be `async`. Nobody writes that, and nothing in the repo does. It is a one-line follow-up if anyone ever hits it. | review oracle (round 2) |

## Escalations

- None.
