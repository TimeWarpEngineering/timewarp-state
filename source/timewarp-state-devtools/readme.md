# TimeWarp.State.DevTools

Blazor Server inspector for a running app's TimeWarp.State actions. It uses TimeWarp.State for its own filter, selection, and pause. The action list is the inspected app's `TimeWarp.State` activities, posted to `POST /ingest/spans`.

This project is not a NuGet package. `TimeWarp.State.DevTools.App` packs its publish output, and an Aspire AppHost adds that executable with `AddTimeWarpStateDevTools()` from `TimeWarp.State.DevTools.Hosting`.

The inspector does not reference `TimeWarp.State.Telemetry` and does not export its own actions. Spans for `InspectorState` are dropped. There is no time-travel control.

## State panel

The page is an action log on the left and **Action**, **State**, and **Diff** tabs on the right.

- **State** shows one store's state types as a collapsible JSON tree (**Tree**) or as text (**Raw**). **Current state** is the fold of the newest store that is not a prerender (the newest store when all are prerenders), so a prerender never replaces the live store. A store selector starts with **Follow live**, then lists stores newest first, with **(untagged)** for spans without a store tag. Picking a store pins it; **Follow live** or **Clear** unpins. A selected row in that store means the fold up to that row (**State after action**).
- Each log row has a store badge: **pre** or **live** from `timewarp.state.render`, otherwise the first four characters of `timewarp.state.store`. The colour is a hash of the full id into eight colours, so stores are usually, not always, distinct. The Action tab shows the full id.
- **Diff** is the structural change against the previous snapshot of that state type in the same store: added, removed, and changed. Equal snapshots say **No change.** A missing snapshot says so. Truncated JSON stays raw and names `MaxSnapshotChars`.
- The filter matches action names, state type names, and the start of a store id or store badge in the log. In the State panel it matches the store badge or a state type name.
- `init` rows (`timewarp.state.kind` = `init`) are state initialization and fold like any snapshot. A prerender and an interactive circuit each produce their own init row. Both stay visible.
- Expanded tree paths stay expanded as new rows arrive.
- The header switches dark, light, and system theme. Dark is the default. Colours come from `wwwroot/tokens.css` (`--twe-*`).
