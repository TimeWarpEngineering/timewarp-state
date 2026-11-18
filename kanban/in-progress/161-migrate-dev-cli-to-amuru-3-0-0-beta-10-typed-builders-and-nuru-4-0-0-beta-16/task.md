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

## Checklist

- [ ] Dev CLI compiles on Amuru 3.0.0-beta.10 / Nuru 4.0.0-beta.16 (`dotnet run --file tools/dev-cli/dev.cs -- self-install`)
- [ ] `./bin/dev build` green, test project links DevCli beta.16 content
- [ ] scripts/*.cs runfiles compile on the new Amuru
- [ ] Holds removed from `.editorconfig`; `ganda repo audit` green

## Session

- Created: 2939415 (2026-11-18), from task 150's implement walk

## Notes

- Origin: task 150 audit gate, 2026-11-18. Error list captured in task 150 `## Results`.
