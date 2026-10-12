# Round 1 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 4 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state-devtools/inspector-log.cs:373
- Description: `OrderStores` sorts by `FirstSequence`, and `EffectiveStoreId` / `FoldCurrent` take `stores[0]`. Picture an app with per-page `@rendermode InteractiveServer` (sample 04's `Counter.razor` is built this way) and more than one interactive page or an interactive layout component. Enhanced navigation fetches the next page over HTTP and prerenders it in a new request scope. That creates a new prerender store, and its `init` rows come after the circuit store's first span. Blazor keeps reusing the existing circuit, which already initialized its states, so the circuit emits no new init. The prerender store becomes "newest", and Current state switches to a store that was thrown away while the user keeps acting on the live one. The requirement says "a prerender snapshot never silently overwrites the live one". This finding comes from reading Blazor's circuit-reuse behaviour; it was not reproduced in a running app.
- Suggestion: When render tags are present, default to the newest store whose `Render` is not `prerender`. Fall back to the newest store otherwise. Add an `InspectorLog` test: interactive store A, then prerender store B, and Current state must stay on A.
- Source: general
- Disposition notes: fixed in 9548c05e

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-devtools/components/pages/Inspector.razor:72
- Description: `Select` always calls `SelectStore(entry.StoreId)`, and `OnStore` pins whatever is chosen. The selector has no "newest" option, so once a user clicks any row, the panel no longer follows the newest store until **Clear**. If the inspected app is then reloaded, the panel stays on the old circuit's store, still labelled **Current state** once the row is deselected. The docs say "Current state is the newest store's fold", which stops being true after a single click.
- Suggestion: Add a "Newest" option (value `""`) at the top of the store selector. Alternatively, have row selection drive the fold through `stateAfterAction` without writing `SelectedStoreId`.
- Source: general
- Disposition notes: fixed in 9548c05e

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-devtools/features/inspector/inspector-state.select-store.cs:279
- Description: An empty `StoreId` means two different things: "follow newest" and "spans with no store tag". The two collide when a log holds both untagged spans (an older telemetry package or a second app) and tagged ones:
  - Clicking an untagged row sets `SelectedStoreId = ""`, which means follow newest. `EffectiveStoreId` then picks a tagged store, so `selectedStore` is false. The panel shows another store's fold under **Current state** instead of the clicked row's **State after action**.
  - If the newest store is the untagged bucket, the `<select value="">` has no matching option, because `""` is filtered out at Inspector.razor:192. The selector then shows a different store from the one on screen.
- Suggestion: Hold the pin in a separate flag or a nullable value, so "no pin" and "the untagged store" are not both `""`. Alternatively, list the untagged bucket in the selector as "(untagged)".
- Source: general
- Disposition notes: fixed in 9548c05e

### M4 — Severity: nit — Status: fixed
- File: source/timewarp-state-devtools/features/inspector/store-badge.cs:406
- Description: `Matches` does a substring search over the full Guid. Any single filter character `0-9` or `a-f`, and short words such as `Add`, `de`, or `Fe`, match almost every row's store id. As a result the action/state filter barely narrows the log while the user types. The State panel has a related problem. Filtering `pre` while the live store is on screen falls through to state-name matching, so the panel is empty and shows "No state snapshots yet. Enable IncludeSnapshots…", which is misleading. The Results section's own "Filtering `pre`" check ends up in this state.
- Suggestion: Match a store id by prefix only, or require four or more characters. For an empty State panel caused by a filter, show a message such as "No state matches the filter".
- Source: general
- Disposition notes: fixed in 9548c05e

### M5 — Severity: suggestion — Status: fixed
- File: tests/timewarp-state-devtools-tests/inspector-log-tests.cs:117
- Description: Several new paths have no test:
  - `FoldThrough` per store, where interleaved rows from the other store must be excluded.
  - `StateRenderKindObserver`: the path with no service registered, and the path where `RendererInfo` throws.
  - The case where a newer store arrives after a pinned or selected store.
  - Clear resetting `SelectedStoreId`.

  The interleaved-ids and fold-per-store requirements are covered at the ingest and `InspectorLog` level.
- Suggestion: Add an `InspectorLog` test for `FoldThrough(sequence)` on interleaved stores, and a small unit test for `StateRenderKindObserver.Observe`.
- Source: general
- Disposition notes: fixed in 9548c05e

### M6 — Severity: nit — Status: fixed
- File: documentation/topics/devtools.md:28
- Description: The docs say "the prerender store and the circuit store stay visually distinct even when both badges read the same word". `StoreBadge.Hue` maps each id into 8 buckets, so two stores share a colour about 1 time in 8. Labels that read the same word, such as two `live` tabs, then look identical apart from the hover id.
- Suggestion: Soften the wording to "usually distinct", or show the id prefix next to `pre`/`live` (for example `live·791f`).
- Source: general
- Disposition notes: fixed in 9548c05e

### M7 — Severity: nit — Status: fixed
- File: source/timewarp-state-devtools/components/pages/Inspector.razor.css
- Description: The badge background colours are hard-coded hex values (`#c5b4ff` … `#79b253`). The rest of the stylesheet and the readme ("Colours come from `wwwroot/tokens.css` (`--twe-*`)") use tokens. In the light theme, `#f7fbff` (hue 6) is close to the page background.
- Suggestion: Define `--twe-store-hue-0..7` in `tokens.css`, with light-theme values.
- Source: general
- Disposition notes: fixed in 9548c05e

### M8 — Severity: nit — Status: fixed
- File: source/timewarp-state-devtools/components/pages/Inspector.razor:201
- Description: `RenderOf(storeId)` calls `Stores()` again, which takes the lock and re-collects up to 500 entries plus the index. A render calls it twice, on top of the `Stores()` call already made in the render block.
- Suggestion: Pass the `stores` list that was already computed into `RenderOf` and `VisibleStates`.
- Source: general
- Disposition notes: fixed in 9548c05e

## Duplicates / conflicts

- None (single reviewer).
