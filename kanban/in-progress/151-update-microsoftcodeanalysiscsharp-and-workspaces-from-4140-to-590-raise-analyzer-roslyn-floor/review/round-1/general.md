# Round 1 — general
**Date:** 2026-11-07
**Scope reviewed:** branch `task/151-update-microsoftcodeanalysiscsharp-and-workspaces` vs merge-base `b406f3a8` (product commit `619b3ef3`), checked against `## Goal (implementer restatement)` and the original requirements.

## Summary

The bump keeps `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.CSharp.Workspaces` equal at 5.9.0. The analyzer, source generator, and code-fix projects still set `PrivateAssets="all"` on those references. `PinRoslynAtTheConsumerCompilerFloor` asserts the `5.9.0` CSharp pin, and its Design region names the SDK floor. The packed `TimeWarp.State.12.0.0-beta.17.nupkg` nuspec lists only `TimeWarp.Mediator`. The three assemblies under `analyzers/dotnet/cs` reference Roslyn 5.9.0.0 (`Microsoft.CodeAnalysis`, `Microsoft.CodeAnalysis.CSharp`, and, for the code fixes, `Microsoft.CodeAnalysis.Workspaces`).

The consumer floor in `readme.md`, `documentation/partials/installation.md`, and `documentation/release-notes/release12.0.0-beta.18.md` matches the SDK toolset pins: .NET 11 preview.6 or later (every current 11 RC is 5.11 or 5.12), and SDK 10.0.400 or later on the .NET 10 line. `eng/Version.Details.props` `MicrosoftNetCompilersToolsetPackageVersion` is 5.6.0-2.26217.103 on v10.0.302 and v10.0.303, 5.9.0-1.26374.102 on v10.0.400, 5.9.0-1.26311.113 on `release/11.0.1xx-preview6`, 5.8.0-1.26312.104 on preview5, 5.7.0-1.26208.110 on preview4, 5.11.0-1.26428.117 on rc1, and 5.12.0-1.26473.112 on the v11.0.100-rc.3.26522.148 tag. The installed compilers match the file versions in Results: SDK 10.0.403 is 5.900.26.52412 (AssemblyVersion 5.9.0.0) and SDK 11.0.100-rc.3 is 5.1200.26.52348 (AssemblyVersion 5.12.0.0). NuGet `Microsoft.CodeAnalysis` 5.9.0 is AssemblyVersion 5.9.0.0, so those compilers sit on the referenced version. The repo `TargetFramework` is `net11.0`, so SDK 10.0.403 cannot build `verify-samples`. Version stays 12.0.0-beta.17, and the breaking item is in the beta.18 notes, which is the next-version note the task asked for. No new `NoWarn` or suppression landed with the bump. `TreatWarningsAsErrors` remains set.

The goal restatement adds "where the SDK can build the sample" to the 10.0.403 `verify-samples` clause. That hedge is recorded under `## Learnings`. It leaves the shipped floor, the pin, and the docs intact, so this round raises no finding.

This pass did not re-run `dev build`, `dev test`, `dev e2e`, or `verify-samples`.

## Issues

None.
