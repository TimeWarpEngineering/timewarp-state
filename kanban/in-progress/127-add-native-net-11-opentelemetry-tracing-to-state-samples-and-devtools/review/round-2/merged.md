# Round 2 — merged findings
**Date:** 2026-10-29
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: kanban/in-progress/127-add-native-net-11-opentelemetry-tracing-to-state-samples-and-devtools/task.md
- Description: The implementation was uncommitted, with no Results and no test run.
- Source: general (round 1)
- Disposition notes: Committed in `b3c309de`. Checklist and Results are written. `./bin/dev test` exits 0 (re-run in round 2).

### M2 — Severity: bug — Status: fixed
- File: samples/04-telemetry/overview.md
- Description: The WASM inspector check under the AppHost was unconfirmed (it went to the DCP port, not the proxy).
- Source: general (round 1)
- Disposition notes: Results record `/counter-wasm` through `http://localhost:5086` with ingest 200 and a live wasm
  row. Accepted on that recorded evidence. The AppHost was not relaunched in round 2.

### M3 — Severity: suggestion — Status: fixed
- File: tests/timewarp-state-devtools-tests/wasm-exporter-tests.cs:49
- Description: No WASM exporter test rejected `Microsoft.AspNetCore.Components`.
- Source: general (round 1)
- Disposition notes: `IgnoreAFrameworkComponentActivity` was added and passes under `./bin/dev test`.

### M4 — Severity: nit — Status: fixed
- File: samples/04-telemetry/server/sample-04-server/native-trace-sources.cs:9
- Description: The Design region called the package's listener "duplication".
- Source: general (round 1)
- Disposition notes: Reworded to "adds a dependency and no spans".

## New findings

None.

## Duplicates / conflicts

- None. Single reviewer.
