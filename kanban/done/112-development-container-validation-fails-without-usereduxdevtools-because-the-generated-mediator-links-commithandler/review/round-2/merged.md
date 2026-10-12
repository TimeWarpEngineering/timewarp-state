# Round 2 — merged findings (final ledger)
**Date:** 2026-10-12
**Sources:** general (round 1), re-verify (round 2)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 2 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: timewarp-state.slnx:4
- Description: The dead `ReduxDevToolsEnabled` solution build type and its per-project mappings were left behind after the csproj `Configurations` entry was removed.
- Suggestion: Remove the build type and every `Solution="ReduxDevToolsEnabled|…"` mapping.
- Source: general
- Disposition notes: Fixed by the review oracle. The build type and all mappings are removed, and projects left with no child elements collapse to self-closing `<Project … />`. `dotnet build timewarp-state.slnx` (Debug and Release) gives 0 errors, `./bin/dev test` is green and `ganda repo audit` exits 0.

### M2 — Severity: nit — Status: wontfix
- File: source/timewarp-state-telemetry/readme.md:112, source/timewarp-state-blazor/readme.md:12
- Description: The package readmes still name the removed APIs in a "removed in 12.0.0-beta.11" sentence.
- Suggestion: Drop the sentences and keep them only in the migration guide.
- Source: general
- Disposition notes: wontfix (orchestrator). The NuGet readme is where beta.10 users upgrading will look, so a one-line pointer to the replacement is intended migration help, not a leftover. The task's leftover-reference rule targets live code and API docs.

### M3 — Severity: nit — Status: wontfix
- File: tests/timewarp-state-tests/architecture/add-timewarp-state-blazor-tests.cs:60
- Description: The test name `ValidateOnBuild_Without_ReduxDevTools` refers to a removed feature.
- Suggestion: Rename it to something like `ValidateOnBuild_Succeeds_For_Development_Host`.
- Source: general
- Disposition notes: wontfix (orchestrator). The task Results and the How to validate section name this exact test, and the name records which regression it guards (CommitHandler linked without UseReduxDevTools). Renaming it would break that trail for no behavior gain.

## Duplicates / conflicts

- None (single reviewer).
