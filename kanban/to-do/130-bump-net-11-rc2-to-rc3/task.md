# Bump .NET 11 RC2 to RC3

## Description

.NET 11 RC3 shipped 2026-10-25. Task 120 (PR #656) moved the repo from RC1 to RC2. Move it from RC2 to RC3. The RC3 SDK `11.0.100-rc.3.26522.148` is installed on this machine (confirmed 2026-10-26), and the RC3 packages `11.0.0-rc.3.26522.148` are on nuget.org.

## Requirements

- `global.json` sdk: `11.0.100-rc.2.26504.105` -> `11.0.100-rc.3.26522.148`
- `Directory.Packages.props`: every `11.0.0-rc.2.26504.105` pin -> `11.0.0-rc.3.26522.148`. On master that is the Microsoft.AspNetCore.Components.* set, Mvc.Testing, TestHost, the Microsoft.Extensions.* set, System.Net.Http.Json, and the analyzer `Microsoft.NETCore.App.Ref` pin. Grep for `rc.2` to catch any other.
- Review the RC3 notes under https://github.com/dotnet/core/tree/main/release-notes/11.0 for breaking changes that touch this repo and fix any fallout.
- CI follows `global.json`; confirm the workflow does not pin an SDK channel separately.

## Checklist

- [ ] Update `global.json` SDK version
- [ ] Update every RC2 package pin to RC3
- [ ] Review RC3 breaking changes and fix any fallout
- [ ] `./bin/dev build` succeeds
- [ ] `./bin/dev test` green
- [ ] `./bin/dev pack` and `./bin/dev verify-samples` green

## Session

- Created: 259f62af-98bf-4133-8bba-2f8a32eb7caa (2026-10-26)

## Notes

Task 120 found no RC2 fallout and changed only `global.json` and `Directory.Packages.props`. Expect the same shape. Since task 129, restores use the isolated `artifacts/nuget-packages` folder, so the first restore after the bump downloads the RC3 set fresh.
