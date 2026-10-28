# Round 2 — merged findings
**Date:** 2026-10-28
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: documentation/release-notes/release12.0.0-beta.16.md:17
- Description (round 1): The consumer-breaking Mediator 15 move had no release note and no version bump.
- Source: general (round 1)
- Disposition notes: Fixed in `22b270f9`. Five **Breaking** items and a migration-page link were added to the unpublished beta.16 note, and each item was checked against the source. The version stays at beta.16 because that version is not on NuGet, and toc.yml already lists it.

### M2 — Severity: nit — Status: fixed
- File: tests/timewarp-state-tests/javascript-interop/json-request-handler-dispatch-tests.cs:188 (also json-request-handler-lifecycle-tests.cs:65, store/store-get-or-add-tests.cs:175)
- Description (round 1): Removing the `CreateStream` members left double blank lines in three fakes.
- Source: general (round 1)
- Disposition notes: Fixed in `22b270f9`.

No new findings on the fix delta.

## Duplicates / conflicts

- None (single reviewer).
