# Round 2 — merged findings
**Date:** 2026-11-05
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: tools/dev-cli/endpoints/workflow/release-pipeline.cs:187
- Description: Round 1: every `RunStepAsync` false, including cancel and throw, aborted as "version already released".
- Suggestion: Return succeeded, failed, cancelled, or threw. Abort with the cancelled or threw reason for those two. On a finished non-zero exit, abort with `CheckVersionAbort.FormatAbortReason`.
- Source: general
- Disposition notes: Verified on the delta. `PipelineConsole.RunStepAsync` returns `StepOutcome`. `ReleasePipeline.CheckVersionFailure` returns null for `Succeeded`, `Abort("Check Version cancelled")` for `Cancelled`, `Abort("Check Version threw")` for `Threw`, and `Abort(CheckVersionAbort.FormatAbortReason(CheckVersionHandler.Outcome, CheckVersionHandler.GitHubPackagesFailure))` for `Failed`. The pin is TimeWarp.Nuru.DevCli 4.0.0-beta.11. `FormatAbortReason` maps `AlreadyReleased` to "version already released", `GitHubPackagesUnreadable` and `NuGetOrgLookupFailed` to their own lines, and every other outcome (including `InvalidInput`, which is what an empty packable set leaves on the handler) to "check-version failed (see the error above)". The handler sets `Outcome` on the instance `RunAsync` passes into `ReleasePipeline`, and `CheckVersionFailure` reads it after `Handle` returns. The PR caller only changed `!bool` to `!= StepOutcome.Succeeded`.

### M2 — Severity: bug — Status: fixed
- File: tools/dev-cli/endpoints/workflow/release-pipeline.cs:86
- Description: Round 1: `GetPackableProjectsAsync` ran while `CurrentStep` was still "Check Version", so its throw or cancel was blamed on check-version.
- Suggestion: Set `CurrentStep` (or call `BeginStep`) to the packable-set work before that call.
- Source: general
- Disposition notes: Verified on the delta. `CurrentStep = "Packable Set"` is the statement immediately before `PackableProjectService.GetPackableProjectsAsync`. It runs only after `CheckVersionFailure` has returned null. `ReleasePipeline.RunAsync` still catches `OperationCanceledException` as `{CurrentStep} cancelled` and any other exception as `{CurrentStep} failed`. `GetPackableProjectsAsync` in the beta.11 content throws `InvalidOperationException` on a bad MSBuild evaluation and passes the token into `CaptureAsync`.

## Duplicates / conflicts

- One general reviewer. Nothing to collapse. No new findings on the fix delta.
