# Round 2 — general
**Date:** 2026-11-11
**Scope reviewed:** delta since round 1 (`b7c9f360..HEAD`): `34ad43c0` fix: collect Cobertura from the e2e test run, `bb9fa831` docs. Product files: `.github/workflows/workflow.yml`, `scripts/e2e.cs`, `scripts/e2e-tests.cs`. Kitchen `task.md` edits read for accuracy.

## Summary

The fix pass makes the e2e `dotnet test` collect coverage for real. It passes
`--collect:XPlat Code Coverage;IncludeTestAssembly=true` and an absolute `--results-directory` at the
repo's `artifacts/coverage`. The upload step now uses `if-no-files-found: error`, and its comment states
the real reason. That resolves round-1 M1 and M2. No new defects found. Risk is low: the upload step
only fails CI when the e2e run passed but wrote no Cobertura file, which is the signal the task wants.

## Prior findings re-verified

- **M1 (bug) — fixed.** `scripts/e2e-tests.cs:21-28` adds the collector and results directory.
  `scripts/e2e.cs:99` resolves `artifacts/coverage` against the repo root (`ScriptContext.FromRelativePath("..")`
  at `scripts/e2e.cs:52` sets the working directory before that line), so the absolute path is right even
  though the `dotnet test` child runs in the test project directory. The PR/merge pipeline always runs E2E
  before the upload (`tools/dev-cli/endpoints/workflow-command.cs:102-115`). Clean runs earlier, and
  verify-samples after E2E does not touch `artifacts/coverage`. Re-ran the filtered command from Results into
  `/tmp/t154-r2-cov-a`. VSTest wrote `<guid>/coverage.cobertura.xml` with `lines-valid="332"` and
  `lines-covered="6"`, matching Results.
- **M2 (nit) — fixed.** `.github/workflows/workflow.yml:121-123` now says release and probe never reach
  the step and that a missing file means the test run wrote nothing.

## Issues

None.

## Observations (not findings)

- The Cobertura report covers only the test assembly (`package name="test-app-end-to-end-tests"`). The SUT
  is a separate process, so no product code is measured. Results states this and why `IncludeTestAssembly`
  is set. `IncludeTestAssembly` is not there to get past the gate: without it the file is still written,
  just with `lines-valid="0"`. Product coverage would need the SUT process instrumented, or the Fixie unit
  suites moved to a collector. That is a possible follow-up task and outside this task's scope, which is to
  clear the audit warnings.
- `PrepareCoverageDirectory` (`scripts/e2e-tests.cs:47-63`) deletes the directory only when the full path
  ends in `/artifacts/coverage`. That is a reasonable guard for a recursive delete.
- The new sentence in the `scripts/e2e.cs` Design region (line 20) is not wrapped like the lines around it.
  Cosmetic, so it is not raised.
- Re-ran `ganda repo audit` (read-only): Passed 42, Failed 1, Skipped 3. The only failure is the
  `kebab-path-names` warning on 7 paths. `kanban-landed` passes. This matches Results.
- `git status` after the build and test run shows no product changes and no deleted tracked wwwroot JS.
- The goal restatement still matches the task. The fix-pass bullet it adds follows round-1 M1 option (a).
  No misread.
