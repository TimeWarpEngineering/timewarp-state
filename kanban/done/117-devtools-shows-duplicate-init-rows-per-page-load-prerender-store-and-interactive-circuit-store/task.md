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

- [x] Telemetry: per-store id tag on init and action spans (+ optional render tag); tests
- [x] Exporter/ingest: `StoreId` on `ActionLogEntry`; tests for two interleaved stores
- [x] Inspector: store badge on rows, full id in Action tab, filter support
- [x] State panel: fold per store with a newest-first store selector
- [x] Docs updated; manual Linux check recorded in `## Results` with a screenshot under `review/`
- [x] `ganda repo audit` clean; devtools + telemetry tests green

## Session

- Created: cockpit (2026-10-12)
- Decision recorded: 2026-10-12 (Steven: show both, attribute to store)
- Implemented: Grok (2026-10-12)
- Reviewed: Claude Opus 5.5 review oracle (2026-10-12), effort 3, 2 rounds, clean
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-12T10:45:29Z

## Results

Both init rows stay in the log. Every init and action span is tagged `timewarp.state.store` with that scope's `Store.Guid`. `AddTimeWarpStateBlazor` registers a scoped `IStateRenderKind`. Components record `RendererInfo.IsInteractive` before `GetState`, and the span then carries `timewarp.state.render` (`prerender` or `interactive`). A host that does not register the service omits the render tag. The row badge reads **pre** or **live** from that tag, otherwise the first four characters of the store id. The colour is a stable hash of the full id. The Action tab shows the full id. The filter matches a store id or badge. Every row stays visible.

The State panel folds one store. Current state is the store that first appeared latest in the log, so a later span from the prerender store leaves the interactive store on screen. The selector lists stores newest first. Diffs stay inside one store.

Package version is `12.0.0-beta.12`. `12.0.0-beta.11` is already on NuGet, and sample 04 restores packages at `TimeWarpStateVersion`, so the sample sees this change only after the version moves.

### How to validate

**Smoke**

```bash
dotnet test tests/timewarp-state-devtools-tests/timewarp-state-devtools-tests.csproj
dotnet test tests/timewarp-state-telemetry-tests/timewarp-state-telemetry-tests.csproj
dotnet test tests/timewarp-state-tests/timewarp-state-tests.csproj
ganda repo audit
```

Manual: pack `12.0.0-beta.12` into `artifacts/packages`, then `aspire start --isolated --non-interactive` on `samples/04-telemetry/apphost`. Open `/counter`, click once, then open DevTools.

**Expect**

DevTools tests: 52 passed. Telemetry tests: 21 passed. State tests: 112 passed, 1 skipped. `ganda repo audit` exits 0 with "Repository passes". The kebab-path advisory is the existing Blazor module filenames and task 110's folder. `./bin/dev check-version --strict` reports source is one prerelease increment ahead of NuGet `12.0.0-beta.11`.

One `/counter` page load shows two init rows with different badges (`pre` and `live`) and different colours. The increment row carries the `live` badge and the same store id as the second init. Current state is the live store (Count 8 after IncrementCount adds 5 to the initial 3). The store selector lists the live store first (`791300f3-12e2-464f-bd33-ffc5036e752c` on the recorded run). Choosing the prerender store shows Count 3. Filtering `pre` leaves the prerender row. The Action tab shows the full store id and Render `interactive`.

Screenshot: `review/page-load-two-stores.png`.

### Review disposition

- Rounds: 2 · effort 3 · roster: general (Claude Opus 5.5 review oracle, general-purpose subagents)
- Final counts: bug 1 fixed · suggestion 3 fixed · nit 5 fixed · 0 open · 0 wontfix
- Disposition: **clean**. Round 1 bug: Current state now defaults to the newest non-prerender store (a store with no render tag counts as live), so an enhanced-navigation prerender cannot replace the live circuit's fold. The selector gained **Follow live** and **(untagged)**. Store filter matches by prefix. Badge colours are `--twe-store-hue-*` tokens. Fixes in 9548c05e plus a test for the no-render-tag rule.
- Artifacts: `review/review-framework.md`, `review/round-1/` and `review/round-2/` (`general.md`, `merged.md`), `review/disposition.md`
- Tests after fixes: devtools 60/60, telemetry 21/21, timewarp-state 116 passed / 1 skipped

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
