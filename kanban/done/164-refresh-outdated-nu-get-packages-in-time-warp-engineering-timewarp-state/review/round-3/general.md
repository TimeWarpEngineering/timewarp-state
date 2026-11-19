# Round 3 — general
**Date:** 2026-11-19
**Scope reviewed:** Resume delta `04cf07c0..005d9b2e` and the product three-dot diff versus `master`. Kitchen `## Goal (implementer restatement)`, `## Results`, and `### How to validate`.

## Summary

Round 2 left M1 fixed and the disposition clean. The only product change since that close is the merge of `origin/master` (task 165). `Directory.Packages.props` keeps the task 164 pins (`Microsoft.Playwright.MSTest.v4` 1.65.0, `Aspire.Hosting` and `Aspire.Hosting.AppHost` 17.0.0, `Aspire.Hosting.Dotnet` 17.0.0) and takes the task 165 comment that `TimeWarpStateVersion` is read from `source/Directory.Build.props`. `msbuild/repository.props` matches master's derivation. The Hosting allow-list still names `Aspire.Hosting.Dotnet`, and the hosting project still references that package with no `PrivateAssets`. `## Results` records the resume build at 0 warnings, both version properties as `12.0.0-beta.19`, `./bin/dev check-version` safe to release, and a clean audit. That record covers the merge. No test file changed in the resume pass. No new defect.

The implementer restatement still says the sample `Aspire.AppHost.Sdk` pin moves to 17.0.0. The task does not ask for that, and the sample removes the obsolete SDK. That misread is already under `## Learnings`.

`Aspire.Hosting.AppHost` 17.0.0 targets (`artifacts/nuget-packages/aspire.hosting.apphost/17.0.0/build/Aspire.Hosting.AppHost.targets`) treat a `ProjectReference` as an Aspire resource only when `IsAspireProjectResource` is `true`. Dropping `IsAspireProjectResource="false"` on the hosting reference, after removing `Aspire.AppHost.Sdk`, leaves that reference as a compile reference. The server is added in `program.cs` with `AddDotnetProject`.

## Issues

None.
