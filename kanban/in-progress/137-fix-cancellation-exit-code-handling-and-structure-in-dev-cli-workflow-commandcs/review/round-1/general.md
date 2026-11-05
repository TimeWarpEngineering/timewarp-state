# Round 1 — general
**Date:** 2026-11-05
**Scope reviewed:** branch `task/137-fix-cancellation-exit-code-handling-and-structure` vs merge-base `4e09ae4c`. Workflow split, cancellation, exit-code handling, props XML errors, and the NuGet API key path.

## Summary

The endpoint is 124 lines and the five services exist. Git, gh, and `dotnet nuget push` take the handler token, and the download and push calls use `WithNoValidation()` so a non-zero exit reaches the "Failed to …" throw. Under the Amuru 3.0.0-beta.1 pin that opt-out is the default; it still documents the intent the task asked for. Malformed props print a line-numbered XML error and abort the release with exit 1. CI passes `NUGET_API_KEY` and no longer puts the key on the `dev` argv; `nuget-login` stays release-only, so pull-request jobs do not receive the key. The release check-version step still collapses every failure into "version already released", and work that runs after that step is labeled as Check Version when it throws or is cancelled.

## Issues

### Issue 1 — Severity: bug
- File: tools/dev-cli/endpoints/workflow/release-pipeline.cs:76
- Description: `RunStepAsync` returns false when the step exits non-zero, when it throws, and when it is cancelled. The release caller treats every false as `Abort("version already released")`. A cancelled check-version prints `Pipeline FAILED — Check Version cancelled` and then `Pipeline ABORTED — version already released`. A throw does the same with "threw". A finished non-zero exit that is not `CheckVersionOutcome.AlreadyReleased` gets that banner too. `CheckVersionCommand.Handler` records `Outcome` and `GitHubPackagesFailure` for this pipeline, and `CheckVersionAbort.FormatAbortReason` in TimeWarp.Nuru.DevCli 4.0.0-beta.11 exists because this hardcoded line sent operators to bump a version that was never published (nuget.org lookup failure, unreadable GitHub Packages, invalid input). The previous handler had the same hardcoded string for a non-zero exit; the new bool return widens it to cancel and throw.
- Suggestion: Have `RunStepAsync` report succeeded, failed, cancelled, or threw. Abort with the cancelled or threw reason for those two. When the handler finishes and `Environment.ExitCode` is non-zero, abort with `CheckVersionAbort.FormatAbortReason(CheckVersionHandler.Outcome, CheckVersionHandler.GitHubPackagesFailure)`.
- Status: open

### Issue 2 — Severity: bug
- File: tools/dev-cli/endpoints/workflow/release-pipeline.cs:81
- Description: `GetPackableProjectsAsync` runs after Check Version returns and before `BeginStep(3, "Locate CI Run")`, while `CurrentStep` is still `"Check Version"`. The service throws `InvalidOperationException` when `dotnet msbuild -getProperty` fails, and it forwards the cancellation token. The release `catch` then prints `Check Version failed: …` or aborts with `Check Version cancelled`, after check-version itself has already succeeded. Before this change that exception left `RunReleaseAsync` with its own message and was not attributed to Check Version.
- Suggestion: Set `CurrentStep` (or call `BeginStep`) to the packable-set work before `GetPackableProjectsAsync`, so a throw or cancel names that work.
- Status: open
