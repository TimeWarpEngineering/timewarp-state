# TimeWarp.State.DevTools.Hosting

Aspire integration that starts the TimeWarp.State DevTools Blazor app and links it from the dashboard.

Dev-only. Production app packages do not reference it. There is no time-travel channel.

## AppHost

```csharp
IResourceBuilder<ProjectResource> devtools = builder.AddTimeWarpStateDevTools();
builder.AddProject<Projects.MyApp>("my-app")
  .WithTimeWarpStateDevTools(devtools);
```

`AddTimeWarpStateDevTools` finds `source/timewarp-state-devtools/timewarp-state-devtools.csproj` by walking up from the AppHost directory. Set `TIMEWARP_STATE_DEVTOOLS_PROJECT` to that csproj when the AppHost is outside this repository.

The DevTools app is not packed into this package. Outside this repository, set `TIMEWARP_STATE_DEVTOOLS_PROJECT` to the full path of a checkout's `source/timewarp-state-devtools/timewarp-state-devtools.csproj`; without it `AddTimeWarpStateDevTools` throws.

The dashboard shows the resource endpoint as **TimeWarp.State DevTools**.

`AddTimeWarpStateDevTools` declares an HTTP endpoint named `http`. Aspire updates an existing endpoint of that name, so a launch profile that already defines `http` is not added twice. When Aspire does not find a launch profile, that declaration is what gives the resource an endpoint and lets Aspire set `ASPNETCORE_URLS`.

Aspire reads launch settings only from `Properties/launchSettings.json` (capital P). The DevTools project keeps its profile there. An external AppHost that points `TIMEWARP_STATE_DEVTOOLS_PROJECT` at a checkout still needs that `Properties/` folder when the app should start as Development with the profile URL. Without the profile the endpoint still exists, the host runs as Production, and static web assets stay attached.

`WithTimeWarpStateDevTools` sets `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` on the inspected resource. The app calls `AddTimeWarpStateDevToolsExporter()` from package `TimeWarp.State.DevTools.Export`. Aspire's own OTLP endpoint is unchanged, so the dashboard still receives traces.

When Aspire ships a dashboard plugin model, this companion app is the page to port.
