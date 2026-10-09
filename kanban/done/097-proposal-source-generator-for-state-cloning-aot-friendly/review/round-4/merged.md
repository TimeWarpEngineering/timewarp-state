# Round 4 — merged findings (verification)
**Date:** 2026-10-10
**Sources:** orchestrator verification (review oracle)
**Scope:** fix delta c124c593..36208023 (M21)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 15 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 4 | 0 |

## Resolved prior

- M1–M20: fixed; re-verified in rounds 2 and 3.
- M21: fixed in 36208023. The orchestrator re-ran the `/tmp/rv097-r3/app` probe variants. All four variants now clone with the correct runtime type, where round 3 saw a runtime throw:
  - GENBASE prints `GD`1 V=1 W=2 same=False`
  - GENIFACE prints `GR`1`
  - GENABS prints `AD`1`
  - GENRESULT prints `Ok`1 sameList=False`
- The generator tests pass 75/75.

## Issues

None new. The fix delta is limited to closing generic subtypes in the planner, plus tests and docs. The shape tests cover the TWSG002 cases for an uninferred type parameter and a failing constraint.
