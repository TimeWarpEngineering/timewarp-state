# Migrate dev CLI to Amuru 3.0.0-beta.10 typed builders and Nuru 4.0.0-beta.16

## Description

`ganda repo audit --fix` on task 150 (2026-11-18) bumped the dev-only pins TimeWarp.Amuru and
TimeWarp.Amuru.Tools 3.0.0-beta.5 → 3.0.0-beta.10, and TimeWarp.Nuru and TimeWarp.Nuru.DevCli
4.0.0-beta.15 → 4.0.0-beta.16. The build then failed:

- Amuru beta.10 ships analyzers AMURU0001 / AMURU0002 that refuse `Shell.Builder("dotnet")` and
  `Shell.Builder("gh")` when a typed builder exists. 17 call sites in `tools/dev-cli/endpoints/`:
  `ci-command.cs` (`dotnet --version`), `e2e-command.cs`, `pack-command.cs` (pack, build, tool, fixie),
  `run-command.cs`, `test-command.cs`, `verify-samples-command.cs` (build, publish, restore),
  `workflow/ci-artifact-service.cs` (`gh api`, `gh run`), `workflow/nuget-push-service.cs`.
  The `scripts/*.cs` runfiles that use `Shell.Builder("dotnet")` probably need the same pass.
- Nuru.DevCli beta.16 `content/any/services/release-guard.cs` uses `GhReleaseCreateBuilder`
  (Amuru.Tools). `tests/timewarp-state-tests` links that file and has no Amuru.Tools reference (CS0246).

Task 150 held the four pins under `package-pin-currency.hold` in `.editorconfig` naming this task, so its
audit stays green. This task removes the holds.

## Requirements

- Move the call sites to the typed builders the analyzers name (`DotNet.Build/Pack/Restore/Publish/Run/Tool`,
  `Gh.Api/Run`). Keep behavior: exit codes, `--no-validation` semantics, working directory, output capture.
- The release promotion path (`ci-artifact-service.cs`) keeps its tests green; read its Design region first.
- Make the test project compile against Nuru.DevCli beta.16 content (add the Amuru.Tools reference or link
  what it needs).
- Delete the four `package-pin-currency.hold` entries and run `ganda repo audit --fix`.

## Goal (implementer restatement)

Move the repo's dev-only pins to TimeWarp.Amuru / Amuru.Tools 3.0.0-beta.10 and TimeWarp.Nuru /
Nuru.DevCli 4.0.0-beta.16 and make everything that uses them compile and behave the same.

Acceptance:
- Every `Shell.Builder("dotnet"|"gh")` the beta.10 analyzers (AMURU0001/0002) refuse, in
  `tools/dev-cli/` and `scripts/*.cs`, uses the typed builder instead, with the same arguments,
  working directory, exit-code handling, `--no-validation` behavior, and output capture.
- `ci-artifact-service.cs` release promotion keeps its Design-region contract; its tests stay green.
- `tests/timewarp-state-tests` compiles against the DevCli beta.16 content (`release-guard.cs`
  needs Amuru.Tools).
- The four `package-pin-currency.hold` entries in `.editorconfig` are gone and `ganda repo audit`
  passes.

## Checklist

- [x] Dev CLI compiles on Amuru 3.0.0-beta.10 / Nuru 4.0.0-beta.16 (`dotnet run --file tools/dev-cli/dev.cs -- self-install`)
- [x] `./bin/dev build` green, test project links DevCli beta.16 content
- [x] scripts/*.cs runfiles compile on the new Amuru
- [x] Holds removed from `.editorconfig`; `ganda repo audit` green

## Session

- Created: 2939415 (2026-11-18), from task 150's implement walk
- 2026-11-18 implement walk: pins moved to Amuru/Amuru.Tools 3.0.0-beta.10 and Nuru/Nuru.DevCli 4.0.0-beta.16;
  dev CLI and scripts moved to the typed builders; holds deleted. `ganda repo audit --fix` changed nothing.

## Results

Pins: `Directory.Packages.props` TimeWarp.Amuru / TimeWarp.Amuru.Tools 3.0.0-beta.5 → 3.0.0-beta.10,
TimeWarp.Nuru / TimeWarp.Nuru.DevCli 4.0.0-beta.15 → 4.0.0-beta.16. The four `package-pin-currency.hold`
entries (and their comment) are gone from `.editorconfig`.

Call-site mapping (argv kept, `WithNoValidation` + exit-code checks kept, working directory kept):

| File | Was | Now |
|------|-----|-----|
| `ci-command.cs` | `dotnet --version` | `DotNet.WithVersion()` |
| `e2e-command.cs` | `dotnet run --file e2e.cs -- …` | `DotNet.Run().WithFile().WithArguments()` (`--` only when there are args; same effect) |
| `pack-command.cs` | pack / tool restore / build / fixie | `DotNet.Pack(p).WithConfiguration().WithOutput().WithProperty("GeneratePackageOnBuild","false")`, `DotNet.Tool().Restore()`, `DotNet.Build(p).WithConfiguration()`, `DotNet.Tool().Run("fixie").WithArguments(…)` |
| `run-command.cs` | `dotnet run --project … --launch-profile … <args>` | `DotNet.Builder().WithArguments(…)` — the catch-all args go to `dotnet run` itself (Design region); `DotNet.Run().WithArguments` would put them after `--` and hand them to the app |
| `test-command.cs` | tool restore, `run --file` | `DotNet.Tool().Restore()`, `DotNet.Run().WithFile()` |
| `verify-samples-command.cs` | build / publish / restore `--configfile --force` / build `--no-restore` | `DotNet.Build/Publish/Restore` typed options; `--configfile` → `WithProperty("RestoreConfigFile", …)` (the MSBuild property behind that switch; the restore builder has no `--configfile`) |
| `workflow/ci-artifact-service.cs` | `gh api`, `gh run download`, `gh run list --json` | `Gh.Api()`, `Gh.Run().Download(id).WithName().WithDirectory()`, `Gh.Run().List().WithWorkflow().WithCommit().WithStatus().WithJson(…)` — same JSON fields, so `DevCliJsonContext` parsing is unchanged; missing `gh` still surfaces as the process-start exception (Gh builders rethrow) |
| `workflow/nuget-push-service.cs` | `dotnet nuget push … [--api-key]` | `DotNet.NuGet().Push(path).WithSource().WithSkipDuplicate()[.WithApiKey()]` |
| `scripts/test.cs` | tool restore + 9× `dotnet fixie <suite>` | `DotNet.Tool().Restore()`, `DotNet.Tool().Run("fixie").WithArguments(suite)` |
| `scripts/e2e-build.cs` | `dotnet msbuild … -getProperty:TargetPath -p:Configuration=Debug` | `DotNet.MsBuild(p).WithGetProperty().WithProperty()` |
| `scripts/e2e-tests.cs` | `dotnet test …` | `DotNet.Test().WithProject().WithNoBuild().WithSettings().WithCollect().WithResultsDirectory()` |
| `scripts/e2e-dev-certs.cs` | `dotnet dev-certs https …` with empty stdin + timeout | `DotNet.Builder()` — `DotNet.DevCerts().Https()` has no stdin or timeout option, and the Design region needs both |

Test project: `tests/timewarp-state-tests` now references `TimeWarp.Amuru.Tools` and has
`global using TimeWarp.Amuru;` so the linked DevCli beta.16 `release-guard.cs` (`GhReleaseCreateBuilder`) compiles.

Gates:

- `dotnet run --file tools/dev-cli/dev.cs -- self-install` → installed `bin/dev` (exit 0).
- `./bin/dev build` → exit 0, 0 errors (includes `timewarp-state-tests`).
- `dotnet build scripts/{build,clean,e2e,package,run-test-app,test}.cs` → each exit 0, no warnings
  (`e2e.cs` pulls in `e2e-build.cs`, `e2e-dev-certs.cs`, `e2e-tests.cs`, `e2e-sut-host.cs`).
- `.githooks/pre-commit.cs`, `.githooks/post-commit.cs` (Amuru.Tools runfiles) → build exit 0.
- Test files: no test file was added or edited. Covering test file
  `tests/timewarp-state-tests/release-pipeline/artifact-source-walk-tests.cs` is a Fixie test, not a standalone
  runfile (no `#if !JARIBU_MULTI`); ran it with `dotnet fixie timewarp-state-tests --configuration Release --tests "*ArtifactSourceWalk*"` → 7 passed.
  The project whose csproj changed, `dotnet fixie timewarp-state-tests --configuration Release` → 159 passed, 1 skipped. CI runs the full suite.
  The dev CLI endpoints (`ci-artifact-service.cs`, pack/verify/run/test/e2e commands) have no unit tests.
- `ganda repo audit --fix` → "Repository passes all audit checks."; it changed no files.

### How to validate

Smoke:

```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install
./bin/dev build
for f in build clean e2e package run-test-app test; do dotnet build scripts/$f.cs || break; done
grep -n "package-pin-currency.hold" .editorconfig
ganda repo audit
```

Expect: self-install and build exit 0; every script builds with no AMURU0001/AMURU0002 errors; the grep
prints nothing; the audit ends with "Repository passes all audit checks." `grep -rn 'Shell.Builder("dotnet"\|Shell.Builder("gh"' tools scripts`
prints nothing.

## Notes

- Origin: task 150 audit gate, 2026-11-18. Error list captured in task 150 `## Results`.
