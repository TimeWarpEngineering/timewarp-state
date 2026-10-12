# TimeWarp.State.DevTools

Blazor Server inspector for a running app's TimeWarp.State actions. It uses TimeWarp.State for its own filter, selection, and pause. The action list is the inspected app's `TimeWarp.State` activities, posted to `POST /ingest/spans`.

This project is not a NuGet package. An Aspire AppHost adds it with `AddTimeWarpStateDevTools()` from `TimeWarp.State.DevTools.Hosting`.

The inspector does not reference `TimeWarp.State.Telemetry` and does not export its own actions. Spans for `InspectorState` are dropped. There is no time-travel control.

## State panel

The page is an action log on the left and **Action**, **State**, and **Diff** tabs on the right.

- **State** shows every state type as a collapsible JSON tree (**Tree**) or as text (**Raw**). Nothing selected means the latest snapshot of each state (**Current state**). A selected row means the fold up to that row (**State after action**).
- **Diff** is the structural change against the previous snapshot of that state type: added, removed, and changed. Equal snapshots say **No change.** A missing snapshot says so. Truncated JSON stays raw and names `MaxSnapshotChars`.
- The filter matches action names and state type names in the log and in the State panel.
- `init` rows (`timewarp.state.kind` = `init`) are state initialization and fold like any snapshot.
- Expanded tree paths stay expanded as new rows arrive.
- The header switches dark, light, and system theme. Dark is the default. Colours come from `wwwroot/tokens.css` (`--twe-*`).
