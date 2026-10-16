---
uid: TimeWarpState:Release.12.0.0-beta.14.md
title: Release 12.0.0-beta.14
---

## Release 12.0.0-beta.14

### DevTools inspector package

- **`TimeWarp.State.DevTools.App`** contains the published inspector. The build is framework-dependent and has no runtime identifier. `TimeWarp.State.DevTools.Hosting` depends on that package, at the same version, and starts `dotnet` on `tools/TimeWarp.State.DevTools.dll`.
- **`AddTimeWarpStateDevTools()`** returns `IResourceBuilder<ExecutableResource>`. It reads the path from assembly metadata stamped by the package targets. It does not walk parent folders and it does not read `TIMEWARP_STATE_DEVTOOLS_PROJECT`.
- **`AddTimeWarpStateDevToolsProject(string projectPath)`** adds the inspector csproj when that path is passed in. That is the inner-loop switch for work on the inspector. The sample AppHost does not call it.
- The inspector resource is excluded from the Aspire publish manifest. Endpoint name `http`, dashboard text **TimeWarp.State DevTools**, and `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` are unchanged.

An AppHost that stored the return value in `IResourceBuilder<ProjectResource>` changes that variable to `IResourceBuilder<ExecutableResource>`.
