# Round 2 — merged findings
**Date:** 2026-09-03
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 5 | 1 |
| nit | 0 | 5 | 0 |

## Resolved prior

Re-verified against fix commit a0e0d707 (see round-2/general.md "Prior findings"):

| ID | Severity | Status | Round-2 verdict |
|----|----------|--------|-----------------|
| M1 | suggestion | fixed | verified (rule matches 10 Plus handlers; test-app's 11 internal handlers need the opt-out) |
| M2 | suggestion | fixed | verified |
| M3 | suggestion | fixed | verified |
| M4 | suggestion | fixed | verified (documented alternative) |
| M5 | suggestion | fixed | verified (CS1591 0, was 376) |
| M6 | suggestion | wontfix | rationale confirmed accurate (tracked since 3deabecc, on origin/dev) |
| M7 | nit | fixed | verified |
| M8 | nit | fixed | verified |
| M9 | nit | fixed | verified |
| M10 | nit | fixed | verified |

## Issues

### M11 — Severity: nit — Status: fixed
- File: source/timewarp-state/extensions/service-collection-extensions.log-timewarp-state-middleware.cs:21
- Description: The method-level comment on `GetComponentOrder` gave a false rationale for the closed-type filter ("open-generic registrations have no concrete type name"); the real reason is that open-generic registrations are inert under the generated mediator.
- Suggestion: Reword to the real reason.
- Source: general
- Disposition notes: Comment reworded to state the inertness rationale (comment-only change, no behavior impact).

## Duplicates / conflicts

- None (single reviewer).
