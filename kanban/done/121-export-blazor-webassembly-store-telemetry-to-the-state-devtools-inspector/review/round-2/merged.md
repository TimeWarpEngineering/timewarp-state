# Round 2 — merged findings
**Date:** 2026-10-16
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 5 | 0 |
| nit | 0 | 3 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs:203
- Description: The packaged inspector ran as Production under Aspire, so ingest CORS never registered.
- Disposition notes: In run mode the variable is set to Development. Unit test plus a launched-dll preflight test (204 with allow-origin). Fixed in ea02ee4a.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-devtools/dev-tools-ingest-cors.cs:108, source/timewarp-state-devtools/span-ingest-endpoint.cs:61
- Description: `UseCors(PolicyName)` was global.
- Disposition notes: `UseCors()` plus `RequireCors` on the ingest route. The private-network header is scoped. A non-ingest route test was added. Fixed in ea02ee4a.

### M3 — Severity: suggestion — Status: fixed
- File: kanban task.md Results (How to validate)
- Description: No browser run was recorded.
- Disposition notes: The AppHost smoke is recorded with **wasm** rows (2026-10-16). The sample host serves the static web assets. Fixed in ea02ee4a.

### M4 — Severity: suggestion — Status: fixed
- File: tests/timewarp-state-devtools-tests/wasm-exporter-tests.cs:48
- Description: The down-inspector test could not fail.
- Disposition notes: It now asserts a throwing, counting handler and `FlushTask.IsCompletedSuccessfully`. Fixed in ea02ee4a.

### M5 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-devtools-wasm/timewarp-state-dev-tools-wasm.cs:195
- Description: Discovery dropped the path base.
- Disposition notes: The relative join keeps the base, and a non-root base test was added. Fixed in ea02ee4a.

### M6 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs:233
- Description: A second inspected app replaced the CORS origins.
- Disposition notes: The annotation-backed callback collects every app's origins, and a two-resource test was added. Fixed in ea02ee4a.

### M7 — Severity: nit — Status: fixed
- File: documentation/release-notes/release12.0.0-beta.15.md
- Description: The shared framework requirement was undocumented.
- Disposition notes: The release note now names it. Fixed in ea02ee4a.

### M8 — Severity: nit — Status: fixed
- File: source/timewarp-state-devtools-wasm/dev-tools-wasm-exporter.cs
- Description: Dead discard statements.
- Disposition notes: Removed. Fixed in ea02ee4a.

### M9 — Severity: nit — Status: fixed
- File: timewarp-state.slnx:2
- Description: The `<Configurations>` block had been removed.
- Disposition notes: Restored. Fixed in ea02ee4a.

## New findings

None.

## Duplicates / conflicts

- Single reviewer. The IDs carry over from round 1, and round 1 stays frozen.
