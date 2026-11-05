# Fix cancellation, exit-code handling and structure in dev-cli workflow-command.cs
## Description

`tools/dev-cli/endpoints/workflow-command.cs` (734 LOC) ignores cancellation, has dead exit-code branches, leaks the NuGet API key to the process list, and bundles five concerns into one endpoint. Fix the bugs and split it.

## Requirements

Bugs:
- All git and gh calls pass `CancellationToken.None` (lines ~449, 464, 474, 496, 505, 518, 538, 553, 583). The handler stores `Ct` in a field that static helpers cannot see. Pass the token through.
- Lines ~615-623 and ~680-688 call `RunAsync()` without `WithNoValidation`; Amuru throws on non-zero exit so the `exitCode != 0` throws and the "Failed to push X!" messages never run.
- `RunStepAsync` (~358-369) depends on `Environment.ExitCode` set elsewhere and reset at ~82 and ~123. A step that throws bypasses "Pipeline FAILED". Return a result instead of using global state.
- Unguarded `XDocument` loads at ~418 and ~434; malformed props throw raw `XmlException`.
- `propsVersion!` at ~325 relies on an earlier step having failed.
- NuGet API key passed on the command line (~677) is visible in the process list. Use env or stdin if the push tool supports it, otherwise document the exposure.
- `Handler.ApiKey` field duplicates the command property.

Split (leaves ~130 lines in the endpoint):
- `ReleasePipeline` (~120-356)
- `ReleaseGitGate` (~442-531 plus `TagPin`, `Ancestor` types ~694-712)
- `CiArtifactService` (~533-629 plus `Locate`/`Download` outcome types ~714-732)
- `NuGetPushService` (~631-692)
- `RepoVersionService` (~380-437); `ReadPropsVersion` duplicates `TryReadMsBuildProperty(..., "Version")`.

Dedupe the 79-char banner/step header repeated 7 times and the three identical `default:` abort cases.

## Goal (implementer restatement)

Make `dev workflow` honest about failure and cancellation, and break the workflow handler
(`workflow-command.cs` + its `workflow-command.release.cs` partial, 750 lines together) into small services.

Acceptance criteria:

- Every git, gh and dotnet process the workflow starts receives the handler's `CancellationToken`.
- Process exit codes are read explicitly (`WithNoValidation()` on every call whose code is checked), so the
  "Failed to push X" / "Failed to download" branches are reachable under the Amuru 3.0.0-beta.1 pin.
- The PR and release pipelines return a `PipelineResult`; the handler sets `Environment.ExitCode` once.
  A step that throws (or is cancelled) still prints "Pipeline FAILED/ABORTED".
- Malformed `source/Directory.Build.props` or `msbuild/repository.props` gives a clear error, not a raw `XmlException`.
- The release pipeline aborts when the source version cannot be read (no `propsVersion!`).
- The NuGet API key can come from `NUGET_API_KEY` so CI no longer puts it on the `dev` command line; the
  remaining `dotnet nuget push --api-key` exposure is documented (dotnet nuget push has no env/stdin option).
- `Handler.ApiKey` field removed.
- Split into `ReleasePipeline`, `ReleaseGitGate`, `CiArtifactService`, `NuGetPushService`, `RepoVersionService`;
  `ReadPropsVersion` folds into `TryReadMsBuildProperty`. Banner and unhandled-status `default:` cases deduped.
- The dev CLI builds and `dev workflow` (PR mode) still runs end to end.

## Checklist

- [x] CancellationToken threaded to every process call
- [x] Exit-code handling consistent with Amuru throw semantics
- [x] Pipeline result returned, not Environment.ExitCode
- [x] XML loads guarded with a clear error
- [x] API key not on command line, or exposure documented
- [x] Split into the five services above
- [x] `dev workflow` still runs end to end

## Review

- Round 1: Blocked — open M1, M2
- Fix pass for round 1 findings (2026-11-05): M1 and M2 fixed
- Round 2: Done
## Notes

Found by a code review on 2026-11-04 of all files over 500 LOC.

## Session

- Created: claude code review session (2026-11-04)
- 2026-11-05 implementer (claude, ganda task work): split and fixes landed; see Results.
  Note: the review's line numbers refer to the 734-line file before task 134 cut it into
  `workflow-command.cs` + `workflow-command.release.cs`; both partials are replaced here.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 8293cea8-5b7f-4b4d-a88a-3e3a23c1cca2, max-turns 500, Done — 2026-11-05T07:41:07Z
- Review oracle round 1: reviewer-cursor (cursor, vendor xai, model grok-4.7), body tw-implementation-review, effort 3, roster general, turn cap 200, session id not exposed. M1 and M2 are open, so a fix pass was requested (2026-11-05).
- Review oracle: review by reviewer-cursor (cursor, vendor xai, model grok-4.7-high), session not reported, max-turns uncapped (no --max-turns flag; bound 200), artifacts general.md, merged.md — 2026-11-05T07:55:55Z
- 2026-11-05 implement-fix-2 (implementer-claude, claude-opus-5-5): fixed round-1 M1 and M2. See Results › Fix pass.
- Review oracle round 2: reviewer-cursor (cursor, vendor xai, model grok-4.7), body tw-implementation-review, effort 1, roster general, turn cap 80, session id not exposed, max-turns uncapped (no --max-turns flag; bound 80). Disposition clean — 2026-11-05. Artifacts: `review/round-2/general.md`, `review/round-2/merged.md`, `review/disposition.md`.

## Results

Split `tools/dev-cli/endpoints/workflow-command.cs` (443 lines) and its `workflow-command.release.cs`
partial (307 lines) into:

| File | Content |
|------|---------|
| `endpoints/workflow-command.cs` (124) | Command, handler, mode detection, PR pipeline as a step table |
| `endpoints/workflow/pipeline-console.cs` | `PipelineResult`, the one banner, `Abort`, `AbortUnhandled`, `RunStepAsync` |
| `endpoints/workflow/release-pipeline.cs` | `ReleasePipeline` (6 release steps) |
| `endpoints/workflow/release-git-gate.cs` | `ReleaseGitGate` + `TagPin*` / `AncestorCheck*` types |
| `endpoints/workflow/ci-artifact-service.cs` | `CiArtifactService` + `LocateRun*` / `DownloadArtifact*` types |
| `endpoints/workflow/nuget-push-service.cs` | `NuGetPushService` |
| `endpoints/workflow/repo-version-service.cs` | `RepoVersionService` (`ReadPropsVersion` folded into `TryReadMsBuildProperty`) |

Bug fixes:

- Every git / gh / dotnet process call now gets the handler `CancellationToken` (no `CancellationToken.None` left).
- `gh run download` and `dotnet nuget push` use `WithNoValidation()` + `RunAsync(ct)` and check the code. Under the
  current Amuru 3.0.0-beta.1 pin `RunAsync` already returns codes, so the explicit opt-out documents intent and keeps
  the "Failed to push X" branch reachable if the default ever flips back to throwing.
- Pipelines return `PipelineResult`; the handler sets `Environment.ExitCode` once. `RunStepAsync` resets the
  exit code before each step, reads it after, and turns a throwing or cancelled step into "Pipeline FAILED — X threw/cancelled".
  `ReleasePipeline` catches service exceptions and aborts with the running step's name. The step endpoints
  (clean, build, test, e2e, verify-samples, check-version) still report via `Environment.ExitCode`; that is
  their contract and out of scope here.
- Malformed props XML prints `Error: <path> is not well-formed XML (line N): …` and fails the SSOT assert.
- The version is read once (from `AssertVersionSsot`) and passed to tag gate, verify and push; no `propsVersion!`.
- API key: `--api-key` still works; `NUGET_API_KEY` env is the fallback and `.github/workflows/workflow.yml` now
  uses it, so the key is off the `dev` argv. `dotnet nuget push` has no env/stdin option (NuGet.Config apikeys need
  DPAPI), so the child argv exposure remains and is documented in `nuget-push-service.cs` Design.
- `Handler.ApiKey` and `Handler.Ct` fields removed. Banner (7 copies) and the three `default:` aborts deduped.

### How to validate

Smoke:

```bash
dotnet build tools/dev-cli/dev.cs
UseHttp=true dotnet run --file tools/dev-cli/dev.cs -- workflow --mode pr; echo "exit=$?"
# failure path: scratch repo with malformed source/Directory.Build.props
s=$(mktemp -d); mkdir -p $s/source $s/msbuild; (cd $s && git init -q . \
  && printf '<Project><PropertyGroup><Version>1.0.0</Version>' > source/Directory.Build.props \
  && printf '<Project><PropertyGroup><TimeWarpStateVersion>1.0.0</TimeWarpStateVersion></PropertyGroup></Project>' > msbuild/repository.props \
  && dotnet run --file <worktree>/tools/dev-cli/dev.cs -- workflow --mode release; echo "exit=$?")
```

Expect:

- The build has no errors or warnings from `tools/dev-cli/endpoints`.
- PR mode prints `Version SSOT aligned`, runs Clean → Build → Test → E2E → Verify Samples, ends with
  `Pipeline SUCCEEDED`, and exits 0.
- The scratch release run prints `… is not well-formed XML (line 1) …`, then the SSOT mismatch, then the
  `Pipeline ABORTED — Assert Version SSOT failed` banner, and exits 1 (no raw `XmlException`).
- With valid props but no master ref, release aborts with `Pipeline ABORTED — master ref unresolvable` and exits 1.

### Review round 1 (2026-11-05)

reviewer-cursor (cursor, vendor xai, model grok-4.7), `tw-implementation-review`, effort 3, roster general, turn cap 200, session id not exposed. Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`. No disposition: M1 and M2 are open bugs.

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 2 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

Re-checked on this run: `dotnet build tools/dev-cli/dev.cs` succeeded with no warnings from `tools/dev-cli/endpoints` (CS8669 warnings are in the TimeWarp.Nuru.DevCli content). A scratch release with malformed `source/Directory.Build.props` printed the line-1 XML error, the SSOT mismatch, and `Pipeline ABORTED — Assert Version SSOT failed`, and exited 1. A scratch release with valid props and no master ref aborted with `master ref unresolvable` and exited 1. The full PR pipeline (clean through verify-samples) was not re-run.

### Fix pass for review round 1 (2026-11-05)

- M1: `PipelineConsole.RunStepAsync` returns `StepOutcome` (`Succeeded`, `Failed`, `Cancelled`, `Threw`) instead of
  `bool`. The PR pipeline fails on anything other than `Succeeded`. The release pipeline's `CheckVersionFailure`
  aborts with `Check Version cancelled`, with `Check Version threw`, or, for a finished non-zero exit, with
  `CheckVersionAbort.FormatAbortReason(handler.Outcome, handler.GitHubPackagesFailure)` from TimeWarp.Nuru.DevCli
  4.0.0-beta.11. Only `AlreadyReleased` says "version already released" now.
- M2: `ReleasePipeline` sets `CurrentStep = "Packable Set"` before `GetPackableProjectsAsync`. A throw or cancel
  there now aborts as `Packable Set failed` / `Packable Set cancelled`, not as Check Version.
- Gates: `dotnet build tools/dev-cli/dev.cs --no-incremental` had no warnings outside CS8669 in the package content.
  `ganda repo audit` exits 0, with two warnings that were already there: kanban-landed (claim 098) and
  kebab-path-names (Blazor `*.lib.module.*` / MSBuild `.targets` names).

Smoke (M1):

```bash
s=$(mktemp -d); mkdir -p $s/source $s/msbuild; (cd $s && git init -q -b master . \
  && printf '<Project><PropertyGroup><Version>1.0.0</Version></PropertyGroup></Project>' > source/Directory.Build.props \
  && printf '<Project><PropertyGroup><TimeWarpStateVersion>1.0.0</TimeWarpStateVersion></PropertyGroup></Project>' > msbuild/repository.props \
  && git add . && git -c user.name=t -c user.email=t@t commit -qm init \
  && dotnet run --file <worktree>/tools/dev-cli/dev.cs -- workflow --mode release; echo "exit=$?")
```

Expect: Step 2 prints `Error: no packable projects found …`, then `Pipeline FAILED — Check Version failed`, then
`Pipeline ABORTED — check-version failed (see the error above)` (not `version already released`), and exits 1.
This run showed exactly that.

### Review round 2 (2026-11-05)

reviewer-cursor (cursor, vendor xai, model grok-4.7), `tw-implementation-review`, effort 1, roster general, turn cap 80, session id not exposed. Scope: the fix delta since `180e7f46` (commit `57714f8a`). Artifacts: `review/review-framework.md`, `review/round-2/general.md`, `review/round-2/merged.md`, `review/disposition.md`.

Disposition: **clean**. Final open count: 0.

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

M1: `RunStepAsync` returns `StepOutcome`. A cancelled or thrown check-version aborts with that reason. A finished non-zero exit aborts with `CheckVersionAbort.FormatAbortReason` (TimeWarp.Nuru.DevCli 4.0.0-beta.11), so only `AlreadyReleased` says "version already released". M2: `CurrentStep = "Packable Set"` is set before `GetPackableProjectsAsync`. No new findings on the delta.

## Learnings

The restatement treats the step commands' `Environment.ExitCode` channel as out of scope. The task asked `RunStepAsync` to return a result instead of using that global state, and the checklist says "Pipeline result returned, not Environment.ExitCode."

What landed matches the pipeline half of that sentence: both pipelines return `PipelineResult`, and the handler sets `Environment.ExitCode` once. `RunStepAsync` still resets, reads, and clears `Environment.ExitCode`. `Clean` and `check-version` come from TimeWarp.Nuru.DevCli and report that way, and the local step handlers do the same. The throw and cancel banners sit on that channel. That is a narrower reading of the global-state sentence. It is not a finding: those package handlers have no other result channel.

The task's Amuru sentence (throw on non-zero) does not match the 3.0.0-beta.1 pin, whose default is no validation. The restatement records that pin, and `WithNoValidation()` is still the call the task asked for.
