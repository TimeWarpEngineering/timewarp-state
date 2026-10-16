# TimeWarp.State.DevTools.Hosting

Aspire integration that starts the TimeWarp.State DevTools inspector and links it from the dashboard.

Dev-only. Production app packages do not reference it. There is no time-travel channel.

## AppHost

```csharp
IResourceBuilder<ExecutableResource> devtools = builder.AddTimeWarpStateDevTools();
builder.AddProject<Projects.MyApp>("my-app")
  .WithTimeWarpStateDevTools(devtools);
```

Reference this package. It depends on `TimeWarp.State.DevTools.App` at the same version. That package contains the published inspector at `tools/TimeWarp.State.DevTools.dll`. The publish is framework-dependent and has no runtime identifier. Aspire starts the dll with the .NET host from `DOTNET_HOST_PATH`, then the running `dotnet` muxer, then `DOTNET_ROOT/dotnet`, then `dotnet` on PATH.

The package targets stamp assembly metadata `timewarpstatedevtoolspath` on the AppHost. `AddTimeWarpStateDevTools` reads that path and adds an executable resource. No source tree, no folder walk, and no environment variable is required.

The dashboard shows the resource endpoint as **TimeWarp.State DevTools**.

The resource declares an HTTP endpoint named `http`. Aspire injects the target port as `ASPNETCORE_HTTP_PORTS`. The resource is excluded from the publish manifest. `WithTimeWarpStateDevTools` does nothing in publish mode, so the inspected app's publish output does not reference the inspector.

`WithTimeWarpStateDevTools` sets `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` on the inspected resource and waits for the inspector when the AppHost is running. The app calls `AddTimeWarpStateDevToolsExporter()` from package `TimeWarp.State.DevTools.Export`. Aspire's own OTLP endpoint is unchanged, so the dashboard still receives traces.

## Inner loop on the inspector

`AddTimeWarpStateDevToolsProject(projectPath)` adds that csproj as a project resource. The path is an argument. The sample AppHost does not call it. Use it only while changing the inspector project itself.

When Aspire ships a dashboard plugin model, this companion app is the page to port.
