---
uid: TimeWarpState:Release.12.0.0-beta.19.md
title: Release 12.0.0-beta.19
---

## Release 12.0.0-beta.19

### Aspire 17 (task 164)

- Aspire.Hosting and Aspire.Hosting.AppHost `13.6.5` → `17.0.0`. Aspire.Hosting is a dependency of the published
  `TimeWarp.State.DevTools.Hosting` package.
- **Breaking:** `AddTimeWarpStateDevToolsProject(projectPath)` returns `IResourceBuilder<DotnetProjectResource>`
  (was `IResourceBuilder<ProjectResource>`). Aspire 17 marks `AddProject(name, path)` obsolete; the method now calls
  `AddDotnetProject` from `Aspire.Hosting.Dotnet` `17.0.0`, which `TimeWarp.State.DevTools.Hosting` now references.
  `AddTimeWarpStateDevTools()` and `WithTimeWarpStateDevTools()` are unchanged.
- Aspire 17 marks `Aspire.AppHost.Sdk` obsolete (ASPIRE012). The telemetry sample AppHost is now a plain
  `Microsoft.NET.Sdk` project with `Aspire.Hosting.AppHost` and `Aspire.Hosting.Dotnet` package references, and adds
  its server with `AddDotnetProject` instead of a `ProjectReference` and `Projects.sample_04_server`. The packaged
  DevTools consumer test builds its AppHost the same way.

### Dependency updates (task 164)

- Test-only pin: Microsoft.Playwright.MSTest.v4 `1.64.1` → `1.65.0`.
