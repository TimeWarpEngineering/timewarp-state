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

## Requirements

Pick and record one in `## Notes` before implementing (the implementer may consult an advisor):

1. **Tag the store instance** — the telemetry handler adds a stable per-store id tag
   (`timewarp.state.store` = a Guid created with the `Store`, or the scope's hash) to every span it
   emits, init and action alike. The inspector groups by store id: it shows the newest store's rows
   by default, folds older stores under a collapsed "previous store (prerender)" header, and the
   State panel folds snapshots per store so a prerender snapshot never overwrites the live one.
   Most honest, also useful for multi-circuit apps; needs an `ActionLogEntry` field and ingest
   mapping. Preferred.
2. **Skip init spans during static render** — in Blazor Server the handler can detect a non-
   interactive render (e.g. `IHttpContextAccessor` with `HttpContext.Response` not yet started, or
   checking whether a `CircuitHandler` scope exists) and not emit init spans then. Cheaper, but
   detection is host-specific and WASM/prerender combinations differ; risky.
3. **Dedupe in the inspector** — collapse consecutive identical `init` snapshots for the same state
   type within a short window. Simplest, but hides real information and breaks when the prerender
   snapshot legitimately differs (e.g. persisted state restored only in the circuit).

Whichever option: the action-count badge and the filter count the visible rows; `init` rows stay
distinguishable (kind tag); ingest tests cover two stores interleaved; manual Linux check with the
sample 04 AppHost records one visible `init` per state after a page load, with the prerender store
reachable but collapsed (option 1) or absent (option 2/3).

## Checklist

- [ ] Direction recorded in Notes
- [ ] Telemetry / exporter change (option 1 or 2) with tests, or inspector dedupe (option 3) with tests
- [ ] Inspector grouping or dedupe UI; State panel folds per the chosen rule
- [ ] Docs: `documentation/topics/devtools.md` explains prerender stores and the `init` rows
- [ ] Manual Linux check recorded in `## Results` with a screenshot under `review/`
- [ ] `ganda repo audit` clean; devtools + telemetry tests green

## Session

- Created: cockpit (2026-10-12)

## Notes

- Related: 113 (init snapshots and state panel), 103 (DevTools app), 058 (telemetry).
- Evidence: task 113 Results ("A static render and the interactive circuit each initialize a store,
  so one page load produces two init rows"); screenshots in 113's and 115's `review/`.
- Source: `source/timewarp-state-telemetry/initialized-state-snapshot-handler.cs`,
  `state-snapshot-recorder.cs`; `source/timewarp-state-devtools/inspector-log.cs`,
  `span-ingest-endpoint.cs`, `action-log-entry.cs`, `components/pages/Inspector.razor`.
