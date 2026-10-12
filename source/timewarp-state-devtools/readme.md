# TimeWarp.State.DevTools

Blazor Server inspector for a running app's TimeWarp.State actions. It uses TimeWarp.State for its own filter, selection, and pause. The action list is the inspected app's `TimeWarp.State` activities, posted to `POST /ingest/spans`.

This project is not a NuGet package. An Aspire AppHost adds it with `AddTimeWarpStateDevTools()` from `TimeWarp.State.DevTools.Hosting`.

The inspector does not reference `TimeWarp.State.Telemetry` and does not export its own actions. Spans for `InspectorState` are dropped. There is no time-travel control.

## State panel

The page is an action log on the left and **Action**, **State**, and **Diff** tabs on the right.

- **State** shows one store's state types as a collapsible JSON tree (**Tree**) or as text (**Raw**). **Current state** is the newest store's fold. A store selector (newest first) switches stores. A selected row in that store means the fold up to that row (**State after action**).
- Each log row has a store badge: **pre** or **live** from `timewarp.state.render`, otherwise the first four characters of `timewarp.state.store`. The colour stays with the full id. The Action tab shows the full id.
- **Diff** is the structural change against the previous snapshot of that state type in the same store: added, removed, and changed. Equal snapshots say **No change.** A missing snapshot says so. Truncated JSON stays raw and names `MaxSnapshotChars`.
- The filter matches action names, state type names, store ids, and store badges in the log. In the State panel it matches the store badge or a state type name.
- `init` rows (`timewarp.state.kind` = `init`) are state initialization and fold like any snapshot. A prerender and an interactive circuit each produce their own init row. Both stay visible.
- Expanded tree paths stay expanded as new rows arrive.
- The header switches dark, light, and system theme. Dark is the default. Colours come from `wwwroot/tokens.css` (`--twe-*`).
