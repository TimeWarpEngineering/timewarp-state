# Round 1 — merged findings
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
- File: tools/dev-cli/endpoints/workflow/release-pipeline.cs:76
- Description: `RunStepAsync` returns false when the step exits non-zero, when it throws, and when it is cancelled. The release caller treats every false as `Abort("version already released")`. A cancelled check-version prints `Pipeline FAILED — Check Version cancelled` and then `Pipeline ABORTED — version already released`. A throw does the same with "threw". A finished non-zero exit that is not `CheckVersionOutcome.AlreadyReleased` gets that banner too. `CheckVersionCommand.Handler` records `Outcome` and `GitHubPackagesFailure` for this pipeline, and `CheckVersionAbort.FormatAbortReason` in TimeWarp.Nuru.DevCli 4.0.0-beta.11 exists because this hardcoded line sent operators to bump a version that was never published (nuget.org lookup failure, unreadable GitHub Packages, invalid input). The previous handler had the same hardcoded string for a non-zero exit; the new bool return widens it to cancel and throw.
- Suggestion: Have `RunStepAsync` report succeeded, failed, cancelled, or threw. Abort with the cancelled or threw reason for those two. When the handler finishes and `Environment.ExitCode` is non-zero, abort with `CheckVersionAbort.FormatAbortReason(CheckVersionHandler.Outcome, CheckVersionHandler.GitHubPackagesFailure)`.
- Source: general
- Disposition notes: fixed in fix pass (2026-11-05). `PipelineConsole.RunStepAsync` returns `StepOutcome` (Succeeded/Failed/Cancelled/Threw). `ReleasePipeline.CheckVersionFailure` aborts with `Check Version cancelled`, `Check Version threw`, or `CheckVersionAbort.FormatAbortReason(Outcome, GitHubPackagesFailure)` for a finished non-zero exit. Scratch-repo smoke: check-version InvalidInput now aborts `check-version failed (see the error above)`, not `version already released`.

### M2 — Severity: bug — Status: fixed
- File: tools/dev-cli/endpoints/workflow/release-pipeline.cs:81
- Description: `GetPackableProjectsAsync` runs after Check Version returns and before `BeginStep(3, "Locate CI Run")`, while `CurrentStep` is still `"Check Version"`. The service throws `InvalidOperationException` when `dotnet msbuild -getProperty` fails, and it forwards the cancellation token. The release `catch` then prints `Check Version failed: …` or aborts with `Check Version cancelled`, after check-version itself has already succeeded. Before this change that exception left `RunReleaseAsync` with its own message and was not attributed to Check Version.
- Suggestion: Set `CurrentStep` (or call `BeginStep`) to the packable-set work before `GetPackableProjectsAsync`, so a throw or cancel names that work.
- Source: general
- Disposition notes: fixed in fix pass (2026-11-05). `CurrentStep = "Packable Set"` is set before `GetPackableProjectsAsync`, so a throw or cancel there reports `Packable Set failed` / `Packable Set cancelled`, not Check Version.

## Duplicates / conflicts

- One general reviewer. Nothing to collapse.
