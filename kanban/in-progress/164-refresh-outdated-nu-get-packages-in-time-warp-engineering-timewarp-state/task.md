# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-state

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-state.
Filter: (all)
Outdated:
  Microsoft.Playwright.MSTest.v4 1.64.1 -> 1.65.0 (dev-only: tests/test-app-end-to-end-tests/test-app-end-to-end-tests.csproj)
  Aspire.Hosting 13.6.5 -> 17.0.0 (library: source/timewarp-state-devtools-hosting/timewarp-state-devtools-hosting.csproj has no PrivateAssets=all)
  Aspire.Hosting.AppHost 13.6.5 -> 17.0.0 (dev-only: samples/04-telemetry/apphost/sample-04-apphost.csproj)
Bump the dev-only pins in this same task, so one release carries them.
Edit Directory.Packages.props in this repo only.

Version: set <Version> in source/Directory.Build.props from 12.0.0-beta.18 to 12.0.0-beta.19 in this pull request. The prerelease suffix is -beta.N, never +beta.N. The wave releases it after merge.

## Session

- Minted by ganda task wave run 20261118T170809Z-b6e473
