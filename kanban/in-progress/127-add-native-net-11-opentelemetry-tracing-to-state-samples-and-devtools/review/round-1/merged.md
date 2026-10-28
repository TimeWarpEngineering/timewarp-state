# Round 1 — merged findings
**Date:** 2026-10-29
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 2 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: kanban/in-progress/127-add-native-net-11-opentelemetry-tracing-to-state-samples-and-devtools/task.md:44
- Description: The implementation is uncommitted: 11 modified files plus the untracked `native-trace-sources.cs`.
  The checklist is all `[ ]`, there is no `## Results`, and no repo audit was run. `.playwright-cli/` scratch is
  untracked at the repo root.
- Suggestion: Run the audit and tests. Commit the product diff, excluding `.playwright-cli/`. Tick the verified
  checklist items and write Results with the span evidence.
- Source: general
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: samples/04-telemetry/overview.md:34
- Description: The WASM inspector check under the AppHost is unconfirmed. The implementer browsed the DCP target
  port (20395) rather than the proxied endpoint (5086) that the CORS origins resolve to, and WASM posts failed CORS.
- Suggestion: Re-verify `/counter-wasm` through the dashboard's endpoint link and record the result. If it still
  fails, prove it is pre-existing on master and open a follow-up task.
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: tests/timewarp-state-devtools-tests/wasm-exporter-tests.cs:15
- Description: No WASM exporter test rejects `Microsoft.AspNetCore.Components`. Only the server exporter test
  covers the framework sources.
- Suggestion: Add a WASM listener test that shows framework activities are not posted.
- Source: general
- Disposition notes:

### M4 — Severity: nit — Status: open
- File: samples/04-telemetry/server/sample-04-server/native-trace-sources.cs:9
- Description: The Design region calls the package's second listener "duplication". The package enriches the same
  activity and creates no duplicate span.
- Suggestion: Reword it to say the package is redundant.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. Single reviewer.
