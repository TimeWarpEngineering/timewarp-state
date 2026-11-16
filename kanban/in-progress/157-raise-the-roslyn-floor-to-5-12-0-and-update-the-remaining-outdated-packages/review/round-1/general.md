# Round 1 — general
**Date:** 2026-11-16
**Scope reviewed:** branch `task/157-raise-the-roslyn-floor-to-5-12-0-and-update-the` vs `master` (product commit `e4c304a8`), against the task requirements and `## Goal (implementer restatement)`.

## Summary

The change sets `Microsoft.CodeAnalysis.Analyzers`, `.CSharp`, and `.CSharp.Workspaces` to 5.12.0, bumps the seven other pins in the task table, restores the lint-only `TimeWarp.SourceGenerators` reference on the two analyzer test projects, and rewrites the consumer floor in the readme, installation partial, analyzers topic, and unreleased beta.18 notes. The goal restatement matches the task. The recorded gates cover build, test, audit, and `ganda nuget outdated`; this review did not re-run them.

The Roslyn floor loads on the installed SDK. `strings -el` on `/home/steve/.local/share/dotnetup/dotnet/sdk/11.0.100/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll` reports `5.12.0-1.26529.118` and `5.12.0.0`. The same command on SDK 10.0.404 reports `5.9.0-1.26529.119`. The restored NuGet `Microsoft.CodeAnalysis.CSharp` 5.12.0 `lib/netstandard2.0` assembly also reports `5.12.0.0`. Reason: the release notes repeat the product version already written in the task, so the check was whether that string was copied. It matches the installed compiler, so this is not a finding. Both test projects' `project.assets.json` restore `TimeWarp.SourceGenerators` 1.0.0-beta.16 with Roslyn 5.12.0. The packed `TimeWarp.State` nuspec lists only `TimeWarp.Mediator`. `TimeWarp.State.DevTools.Export` depends on `OpenTelemetry` 1.21.0. Analyzer, code-fix, and generator projects stay `netstandard2.0` with `PrivateAssets="all"`.

## Issues

### Issue 1 — Severity: suggestion
- File: readme.md:44
- Description: The packed readme and `documentation/partials/installation.md:8` say ".NET 11 preview and RC SDKs are not supported." The task decision, the pin comment in `Directory.Packages.props`, and `documentation/release-notes/release12.0.0-beta.18.md` limit that exclusion to preview and RC SDKs that bundle an older Roslyn. Task 151 recorded that SDK 11.0.100-rc.3 bundles Roslyn 5.12 (toolset pin `5.12.0-1.26473.112`, AssemblyVersion 5.12.0.0), so the blanket sentence overstates the floor the rest of this change documents. The same sentence is inside `artifacts/packages/TimeWarp.State.12.0.0-beta.18.nupkg` `readme.md`.
- Suggestion: Use the decision's qualifier in `readme.md` and `documentation/partials/installation.md`: preview and RC SDKs that bundle an older Roslyn are not supported. Leave the minimum at .NET SDK 11.0.100 (GA).
- Status: open
