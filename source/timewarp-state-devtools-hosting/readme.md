# TimeWarp.State.DevTools.Hosting

Aspire integration that starts the TimeWarp DevTools inspector and links it from the dashboard.

Dev-only. Production app packages do not reference it. There is no time-travel channel.

## AppHost

```csharp
IResourceBuilder<ExecutableResource> devtools = builder.AddTimeWarpStateDevTools();
builder.AddProject<Projects.MyApp>("my-app")
  .WithTimeWarpStateDevTools(devtools);
```

Reference this package. It depends on `TimeWarp.State.DevTools.App` at the same version. That package contains the published inspector at `tools/TimeWarp.State.DevTools.dll`. The publish is framework-dependent and has no runtime identifier. Aspire starts the dll with the .NET host from `DOTNET_HOST_PATH`, then the running `dotnet` muxer, then `DOTNET_ROOT/dotnet`, then `dotnet` on PATH.

The package targets stamp assembly metadata `timewarpstatedevtoolspath` on the AppHost. `AddTimeWarpStateDevTools` reads that path and adds an executable resource. No source tree, no folder walk, and no environment variable is required.

The default resource name is `timewarp-devtools`. The dashboard shows the resource endpoint as **TimeWarp DevTools**. A caller can pass a different name to `AddTimeWarpStateDevTools`.

The resource declares an HTTP endpoint named `http`. Aspire injects the target port as `ASPNETCORE_HTTP_PORTS`. In run mode the executable also gets `ASPNETCORE_ENVIRONMENT=Development`. The published `tools/` folder has no launch profile, and ingest CORS is registered only in Development. Publish mode does not set that variable. The resource is excluded from the publish manifest. `WithTimeWarpStateDevTools` does nothing in publish mode, so the inspected app's publish output does not reference the inspector.

`WithTimeWarpStateDevTools` sets `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` on the inspected resource and waits for the inspector when the AppHost is running. The app calls `AddTimeWarpStateDevToolsExporter()` from package `TimeWarp.State.DevTools.Export`. Aspire's own OTLP endpoint is unchanged, so the dashboard still receives traces.

The same call adds the inspected app's `https` and `http` endpoints to `TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS` on the inspector. A later `WithTimeWarpStateDevTools` on another app adds that app's origins to the same list. The inspector allows browser posts only from those origins, and only in Development, and only on `POST /ingest/spans`. Publish mode adds neither variable. The browser reads the origin from `GET {base}/_timewarp/state-devtools` on the inspected app (`MapTimeWarpStateDevToolsClientConfig`), including a path base, and posts with `TimeWarp.State.DevTools.Wasm`.

## Inner loop on the inspector

`AddTimeWarpStateDevToolsProject(projectPath)` adds that csproj as a project resource. The path is an argument. The sample AppHost does not call it. Use it only while changing the inspector project itself.

When Aspire ships a dashboard plugin model, this companion app is the page to port.
