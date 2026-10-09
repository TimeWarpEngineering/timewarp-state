# Round 2 — general
**Date:** 2026-10-10
**Scope reviewed:** Fix delta on top of f27db723: the two registration extensions, five sample overviews, and the new add-timewarp-state-blazor-tests.cs. Re-verified M1–M4.

## Summary

M1, M3 and M4 are fixed as described. `TryAddScoped<JsonRequestHandler>()` is idempotent with `AddTimeWarpStateBlazor`, and the tests assert that exactly one descriptor is registered. Overview edits keep CRLF endings and match the sample `program.cs` files. M2 remains wontfix, with its rationale in round-1 merged.md. `./bin/dev test` exits 0. No new issues.

## Issues

<!-- none -->
