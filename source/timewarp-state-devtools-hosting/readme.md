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

The dashboard shows the resource endpoint as **TimeWarp.State DevTools**.

`WithTimeWarpStateDevTools` sets `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` on the inspected resource. The app calls `AddTimeWarpStateDevToolsExporter()` from package `TimeWarp.State.DevTools.Export`. Aspire's own OTLP endpoint is unchanged, so the dashboard still receives traces.

When Aspire ships a dashboard plugin model, this companion app is the page to port.
