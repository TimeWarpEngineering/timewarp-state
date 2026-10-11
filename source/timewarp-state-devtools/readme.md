# TimeWarp.State.DevTools

Blazor Server inspector for a running app's TimeWarp.State actions. It uses TimeWarp.State for its own filter, selection, and pause. The action list is the inspected app's `TimeWarp.State` activities, posted to `POST /ingest/spans`.

This project is not a NuGet package. An Aspire AppHost adds it with `AddTimeWarpStateDevTools()` from `TimeWarp.State.DevTools.Hosting`.

The inspector does not reference `TimeWarp.State.Telemetry` and does not export its own actions. Spans for `InspectorState` are dropped. There is no time-travel control.
