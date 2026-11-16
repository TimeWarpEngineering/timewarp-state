# Round 1 — general
**Date:** 2026-11-16
**Scope reviewed:** branch `task/156-bring-package-pins-current-so-the-merge-audit` vs `master` (merge-base `0c625d94`), read against the original task and `## Goal (implementer restatement)`.

## Summary

The branch brings the TimeWarp pins to the versions the task named (Mediator and Mediator.Generators 15.0.0-beta.9, Amuru and Amuru.Tools 3.0.0-beta.5, Nuru and Nuru.DevCli 4.0.0-beta.14, SourceGenerators 1.0.0-beta.16, Terminal 2.0.0-beta.2, Build.Tasks 2.0.1-beta.1). `ganda repo audit --fix` is its own commit (`b7a570a9`) before the .NET 11 GA, Aspire 13.6.5, and State `12.0.0-beta.18` commit. The restatement matches the task, including the mechanical-or-report rule for Build.Tasks. No misread.

The recorded gates cover the change: `./bin/dev build` 0 warnings / 0 errors, `./bin/dev test` exit 0 with per-project counts, `ganda repo audit` exit 0 with `package-pin-currency` PASS, and `./bin/dev check-version` reporting 12.0.0-beta.18 as new. This review did not re-run those gates.

Build.Tasks 2.0.1-beta.1 is mechanical on this repo. Its `build/TimeWarp.Build.Tasks.props` still stamps `CommitHash` and `CommitDate`, and it errors with `TWBT0001` only when MSBuild is not the .NET 11 `dotnet` host. `global.json` pins SDK `11.0.100` and `.github/workflows/workflow.yml` installs that SDK from `global.json`. The two floor test projects drop the lint-only `TimeWarp.SourceGenerators` reference because that package's nuspec depends on `Microsoft.CodeAnalysis.CSharp` 5.12.0 for `net11.0` only. The analyzer and generator projects stay on `netstandard2.0` and on the 5.9.0 pin. `PinRoslynAtTheConsumerCompilerFloor` was not edited. The already-packed `TimeWarp.State.12.0.0-beta.18.nupkg` depends on Mediator 15.0.0-beta.9 and does not list Roslyn.

One contract hole remains: the readme packed into that nupkg still tells hosts to consume Mediator 15.0.0-beta.5.

Targeted check, not a gate: read the cached `TimeWarp.SourceGenerators` 1.0.0-beta.16 and `TimeWarp.Build.Tasks` 2.0.1-beta.1 nuspecs and props under `artifacts/nuget-packages/`, and the dependency elements inside `artifacts/packages/TimeWarp.State*.12.0.0-beta.18.nupkg`. Reason: the Results claim the SourceGenerators collision is limited to the two floor test projects and that Build.Tasks 2.x was mechanical, and the readme still names the old Mediator pin.

## Issues

### Issue 1 — Severity: bug
- File: readme.md:26
- Description: `source/timewarp-state/timewarp-state.csproj` packs `$(RepositoryRoot)readme.md` as the package readme. That sentence says hosts consume TimeWarp.Mediator 15.0.0-beta.5. The `TimeWarp.State.12.0.0-beta.18.nupkg` nuspec depends on `TimeWarp.Mediator` version `15.0.0-beta.9` (minimum). A host that pins the version the readme names fails restore against this package. The same current-version sentence is still in `claude.md:80`, `claude.md:170`, `documentation/overview.md:44`, and `documentation/partials/summary.md:10`. Release notes for beta.18 already state the bump. Historical notes, the `15.0.0-beta.5 or later` feature sentences (the attribute behavior did not change), and the sample nuspec XML in `nuspec-dependency-allow-list-tests.cs` are not this bug.
- Suggestion: Change those five current-version sentences to 15.0.0-beta.9 so the packed readme matches the nuspec. Leave the "or later" feature history and the older release notes as they are.
- Status: open
