---
uid: TimeWarpState:Release.12.0.0-beta.18.md
title: Release 12.0.0-beta.18
---

## Release 12.0.0-beta.18

### Consumer compiler floor raised to Roslyn 5.9 (task 151)

- **Breaking for old compilers:** the analyzers, code fixes and source generators inside `TimeWarp.State` now compile against `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.CSharp.Workspaces` **5.9.0** (was 4.14.0). The compiler that builds your project must be Roslyn 5.9 or later.
- **Minimum SDK:** .NET 11 SDK **11.0.100-preview.6** or later (every 11 RC qualifies). On the .NET 10 line, Roslyn 5.9 first ships in SDK **10.0.400**. 10.0.3xx carries Roslyn 5.6 and does not load the analyzers. TimeWarp.State 12 packages target `net11.0`, so a .NET 11 SDK is already required to consume them.
- Floor checked against the SDKs' own Roslyn pins (`eng/Version.Details.props` in dotnet/sdk): 10.0.302 has 5.6.0, 10.0.400 has 5.9.0, 11.0.100-preview.5 has 5.8.0, preview.6 has 5.9.0, rc.3 has 5.12.
- An older compiler reports **CS9057** (the analyzer references a newer compiler) and runs none of the TimeWarp.State analyzers or generators. Upgrade the SDK in `global.json`.
- Roslyn is still a compile-only reference (`PrivateAssets="all"`). It is not a package dependency of any TimeWarp.State package.

### Dependency updates (task 156)

- .NET 11 GA: Microsoft.AspNetCore.Components.*, Microsoft.Extensions.* and related pins `11.0.0-rc.3.26522.148` → `11.0.0`. `global.json` now pins SDK `11.0.100`. The rc.3 `Microsoft.Extensions.Validation` carried advisory GHSA-m28h-pj36-4364.
- TimeWarp.Mediator and TimeWarp.Mediator.Generators `15.0.0-beta.5` → `15.0.0-beta.9`. Mediator beta.9 builds its generator and analyzer for `net11.0` only, so the build host must be the .NET 11 `dotnet` CLI. There were no API changes.
- Aspire.Hosting, Aspire.Hosting.AppHost and the sample `Aspire.AppHost.Sdk` `13.6.4` → `13.6.5` (13.6.4 carried advisory GHSA-f7vh-h5g3-7jrh).
- Build-only pins: TimeWarp.Amuru `3.0.0-beta.5`, TimeWarp.Nuru `4.0.0-beta.14`, TimeWarp.SourceGenerators `1.0.0-beta.16`, TimeWarp.Terminal `2.0.0-beta.2`, TimeWarp.Build.Tasks `2.0.1-beta.1`. None of them is a dependency of a published package.
- The analyzer and source generator Roslyn floor stays at 5.9.0.
