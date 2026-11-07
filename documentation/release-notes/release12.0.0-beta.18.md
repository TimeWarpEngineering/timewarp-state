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
