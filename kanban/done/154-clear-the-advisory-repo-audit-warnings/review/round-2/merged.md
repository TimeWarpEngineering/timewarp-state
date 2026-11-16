# Round 2 — merged findings
**Date:** 2026-11-11
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: .github/workflows/workflow.yml:120, scripts/e2e-tests.cs:21
- Description (round 1): the coverage upload step uploaded a directory that nothing wrote.
- Disposition notes: fixed in `34ad43c0`. The e2e `dotnet test` collects XPlat Code Coverage
  (IncludeTestAssembly=true) into the absolute `artifacts/coverage`. The upload uses `if-no-files-found: error`.
  Round 2 re-ran the collection and got `coverage.cobertura.xml` with `lines-valid="332"`.

### M2 — Severity: nit — Status: fixed
- File: .github/workflows/workflow.yml:121
- Description (round 1): the step comment gave the wrong reason for the empty directory.
- Disposition notes: fixed in `34ad43c0`. The comment now says a missing file means the e2e run wrote nothing.

## New findings

None.

## Duplicates / conflicts

- None.

## Follow-up (outside this task)

- The report measures only the e2e test assembly, because the SUT runs out of process. Measuring
  product coverage (instrumenting the SUT, or collecting coverage from the Fixie unit suites) would need
  its own task.
