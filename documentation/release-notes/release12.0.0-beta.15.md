---
uid: TimeWarpState:Release.12.0.0-beta.15.md
title: Release 12.0.0-beta.15
---

## Release 12.0.0-beta.15

### WebAssembly store spans in DevTools

- **`TimeWarp.State.DevTools.Wasm`** posts `TimeWarp.State` activities from the browser to the inspector. It listens with `ActivityListener` and sends with `HttpClient.SendAsync`. It does not reference OpenTelemetry. A missing origin, or a host that is not Development, registers nothing.
- The server that hosts the WebAssembly app maps `GET /_timewarp/state-devtools` in Development with `MapTimeWarpStateDevToolsClientConfig()`. The body is the `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` origin.
- The inspector allows `POST /ingest/spans` from configured origins only, and only in Development. Allow headers are on that route only. `WithTimeWarpStateDevTools` adds each inspected app's http and https endpoints to `TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS`. A second inspected app keeps the first app's origins.
- In run mode, `AddTimeWarpStateDevTools` sets `ASPNETCORE_ENVIRONMENT=Development` on the packaged inspector. The published `tools/` folder has no launch profile, and ingest CORS is registered only in Development. Publish mode does not set the variable.
- `TimeWarp.State.DevTools.Export` references the shared framework `Microsoft.AspNetCore.App` for `MapTimeWarpStateDevToolsClientConfig`. That is not a NuGet package dependency. A console or worker host that references the package needs the ASP.NET Core shared framework at runtime.
- Spans carry `timewarp.state.runtime` (`server` or `wasm`). The log shows **srv** or **wasm** beside the store badge.
- The wire document is unchanged. A server span that already has a runtime tag keeps it.
- The browser discovers the inspector with `GET {base}/_timewarp/state-devtools`, where `{base}` is the host base address, including a path base.

Reference the Wasm package from Debug builds and wrap the call in `#if DEBUG` so a published bundle does not contain it. The telemetry sample's `/counter-wasm` page keeps the reference visible.
