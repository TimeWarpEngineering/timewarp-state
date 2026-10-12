# DevTools shows duplicate init rows per page load: prerender store and interactive circuit store

## Description

Since task 113 (PR #637) the telemetry package emits a `{StateType}.Initialized` span
(`timewarp.state.kind = init`, `state.snapshot` event) when a state is initialized, so the DevTools
State panel is populated before any action. In a Blazor Server app with prerendering, one page load
produces **two** `init` rows per state: the static server render builds a scoped service provider
and store, initializes the states, and throws the store away; then the interactive circuit builds a
second store and initializes them again. Task 113's Results and Steven's screenshot both show it
(`init`, `init`, then the action). Every navigation that prerenders repeats it. The rows are
correct, but they double the noise and the first store's snapshot is never the one the user acts on.

Filed by Claude (cockpit) at Steven's request, 2026-10-12.

## Decision

Steven, 2026-10-12: "maybe the two init should display if they both actually go through the
pipeline?" Yes — both stores are real and both initialize through the pipeline, so the inspector
**shows both** and makes clear which store each row belongs to. Do not skip prerender spans and do
not dedupe rows; the log must reflect what happened.

## Requirements

- **Store identity on every span.** The telemetry package tags each span it emits (init and action)
  with a stable per-store id, e.g. `timewarp.state.store` = a Guid created with the `Store` instance
  (or the DI scope). The exporter forwards it; ingest maps it to a new `ActionLogEntry.StoreId`.
- **Legible in the log.** Rows carry a short store badge (first 4-6 chars of the id, colour-stable
  per store so the prerender store and the circuit store look different at a glance). Hovering or
  the Action tab shows the full id. Where the host can tell, add `timewarp.state.render` =
  `prerender` | `interactive` as an extra tag so the badge can read "pre" / "live"; when it cannot,
  the id alone is enough. No rows are hidden by default.
- **State panel folds per store.** "Current state" is the newest store's fold; a store selector
  (dropdown or tabs, newest first) switches it, so a prerender snapshot never silently overwrites
  the live one and the user can still inspect what the prerender store held.
- **Filter** can match a store id/badge.
- Ingest tests: two stores interleaved keep their ids; fold is per store. Manual Linux check with
  the sample 04 AppHost: one page load shows two `init` rows with different badges, the action row
  carries the live store's badge, and the State panel shows the live store by default.
- Docs: `documentation/topics/devtools.md` explains prerender vs interactive stores and the badge.

## Checklist

- [ ] Telemetry: per-store id tag on init and action spans (+ optional render tag); tests
- [ ] Exporter/ingest: `StoreId` on `ActionLogEntry`; tests for two interleaved stores
- [ ] Inspector: store badge on rows, full id in Action tab, filter support
- [ ] State panel: fold per store with a newest-first store selector
- [ ] Docs updated; manual Linux check recorded in `## Results` with a screenshot under `review/`
- [ ] `ganda repo audit` clean; devtools + telemetry tests green

## Session

- Created: cockpit (2026-10-12)
- Decision recorded: 2026-10-12 (Steven: show both, attribute to store)

## Notes

- Related: 113 (init snapshots and state panel), 115 (Action tab facts), 103 (DevTools app), 058 (telemetry).
- Evidence: task 113 Results ("A static render and the interactive circuit each initialize a store,
  so one page load produces two init rows"); screenshots in 113's and 115's `review/`.
- Source: `source/timewarp-state-telemetry/initialized-state-snapshot-handler.cs`,
  `telemetry-behavior.cs`, `state-snapshot-recorder.cs`; `source/timewarp-state-devtools-export/`;
  `source/timewarp-state-devtools/inspector-log.cs`, `span-ingest-endpoint.cs`, `action-log-entry.cs`,
  `components/pages/Inspector.razor`.
- Rejected: skipping init spans during static render, and deduping identical init rows in the
  inspector — both hide real pipeline activity.
