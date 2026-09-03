# Round 4 — merged findings (re-verification)
**Date:** 2026-09-03
**Sources:** general
**Scope:** fix commit 2d9fec36 vs round-3 ledger M12–M16, plus scan of the fix delta.

## Counts (rounds 3–4, CI delta)

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 3 | 0 |

## Resolved prior

| ID | Severity | Status | Round-4 verdict |
|----|----------|--------|-----------------|
| M12 | suggestion | fixed | verified (truth table run: unset/garbage → true, "false"/"FALSE" → false) |
| M13 | suggestion | fixed | verified (harness: exit 1 with suite named on thrown exception and on non-zero return) |
| M14 | suggestion | fixed | verified (`CI == "true"` is the GitHub Actions value; both consuming jobs restore actions/cache first) |
| M15 | nit | fixed | verified (guard present in all five runfiles; build.cs clean recreates artifacts/packages) |
| M16 | nit | fixed | verified (already-exited case with live async readers returns immediately, output flushed) |

## Issues

### M17 — Severity: nit — Status: fixed
- File: scripts/build.cs:96
- Description: The M14 guard was applied to clean.cs and package.cs but build.cs's `clean` route still clears NuGet locals unconditionally. Not live (CI invokes build.cs with the default route only) but a latent reintroduction of M14.
- Suggestion: apply the same `CI == "true"` guard.
- Source: general
- Disposition notes: Guard applied to `CleanSolution` in build.cs (same shape as clean.cs/package.cs). Verified by the review oracle: `dotnet build ./scripts/build.cs` succeeds with only the pre-existing CS0219; the change is a copy of the round-4-verified M14 pattern, so no round 5 was opened.

## Duplicates / conflicts

- None (single reviewer).
