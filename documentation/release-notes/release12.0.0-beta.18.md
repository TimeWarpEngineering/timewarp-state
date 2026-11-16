---
uid: TimeWarpState:Release.12.0.0-beta.18.md
title: Release 12.0.0-beta.18
---

## Release 12.0.0-beta.18

### Consumer compiler floor raised to Roslyn 5.12 (tasks 151, 157)

- **Breaking for old compilers:** the analyzers, code fixes and source generators inside `TimeWarp.State` now compile against `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.CSharp.Workspaces` **5.12.0** (was 4.14.0). The compiler that builds your project must be Roslyn 5.12 or later.
- **Minimum SDK:** .NET SDK **11.0.100** (the .NET 11 GA SDK) or later. It bundles Roslyn 5.12.0 (`Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll` reports `5.12.0-1.26529.118`). TimeWarp.State 12 packages target `net11.0`, so a .NET 11 SDK is already required to consume them. .NET 11 preview and RC SDKs that bundle an older Roslyn are not supported. No .NET 10 SDK can build `net11.0`; SDK 10.0.404, for reference, bundles Roslyn 5.9.0.
- An older compiler reports **CS9057** (the analyzer references a newer compiler) and runs none of the TimeWarp.State analyzers or generators. Upgrade the SDK in `global.json`.
- Roslyn is still a compile-only reference (`PrivateAssets="all"`). It is not a package dependency of any TimeWarp.State package.

### Dependency updates (task 156)

- .NET 11 GA: Microsoft.AspNetCore.Components.*, Microsoft.Extensions.* and related pins `11.0.0-rc.3.26522.148` → `11.0.0`. `global.json` now pins SDK `11.0.100`. The rc.3 `Microsoft.Extensions.Validation` carried advisory GHSA-m28h-pj36-4364.
- TimeWarp.Mediator and TimeWarp.Mediator.Generators `15.0.0-beta.5` → `15.0.0-beta.9`. Mediator beta.9 builds its generator and analyzer for `net11.0` only, so the build host must be the .NET 11 `dotnet` CLI. There were no API changes.
- Aspire.Hosting, Aspire.Hosting.AppHost and the sample `Aspire.AppHost.Sdk` `13.6.4` → `13.6.5` (13.6.4 carried advisory GHSA-f7vh-h5g3-7jrh).
- Build-only pins: TimeWarp.Amuru `3.0.0-beta.5`, TimeWarp.Nuru `4.0.0-beta.14`, TimeWarp.SourceGenerators `1.0.0-beta.16`, TimeWarp.Terminal `2.0.0-beta.2`, TimeWarp.Build.Tasks `2.0.1-beta.1`. None of them is a dependency of a published package.

### Dependency updates (task 157)

- Microsoft.CodeAnalysis.Analyzers, Microsoft.CodeAnalysis.CSharp and Microsoft.CodeAnalysis.CSharp.Workspaces `5.9.0` → `5.12.0` (the floor above).
- OpenTelemetry `1.20.1` → `1.21.0`. This is a dependency of the published `TimeWarp.State.DevTools.Export` package. The telemetry sample's OpenTelemetry.Extensions.Hosting and OpenTelemetry.Exporter.OpenTelemetryProtocol move to `1.21.0` too.
- Test-only pins: Microsoft.NET.Test.Sdk `18.11.0` → `18.12.0`, MSTest.TestAdapter and MSTest.TestFramework `4.5.2` → `4.5.3`, coverlet.collector `10.1.0` → `10.1.1`.
- The analyzer and source generator test projects lint with TimeWarp.SourceGenerators again; task 156 had removed it there because beta.16 needs Roslyn 5.12.
