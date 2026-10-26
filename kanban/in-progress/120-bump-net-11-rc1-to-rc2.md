# Bump .NET 11 RC1 to RC2

## Description

.NET 11 RC2 shipped 2026-10-13 (go-live). Move the repo from RC1 to RC2. The RC2 SDK is installed on Windows, WSL and the box.

## Requirements

- `global.json` sdk: `11.0.100-rc.1.26425.128` -> `11.0.100-rc.2.26504.105`
- `Directory.Packages.props`: Microsoft.AspNetCore.Components.* and Microsoft.AspNetCore.Mvc.Testing `11.0.0-rc.1.26425.128` -> `11.0.0-rc.2.26504.105` (and any other RC1 versions)
- Review ASP.NET Core RC2 breaking-change notes: https://github.com/dotnet/core/tree/main/release-notes/11.0

## Checklist

- [ ] Update global.json SDK version
- [ ] Update AspNetCore.Components.* and Mvc.Testing package versions
- [ ] Review RC2 breaking changes and fix any fallout
- [ ] Build succeeds
- [ ] Tests green

## Session

- Created: 2026-10-14
