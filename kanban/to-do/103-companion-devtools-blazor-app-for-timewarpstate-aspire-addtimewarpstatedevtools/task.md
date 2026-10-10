# Task 103: Companion DevTools Blazor app for TimeWarp.State (Aspire AddTimeWarpStateDevTools)

## Description

Build a companion DevTools app for TimeWarp.State: a Blazor app that dogfoods TimeWarp.State and
shows a running app's action log and state. It is added to an Aspire AppHost as a resource through
an `AddTimeWarpStateDevTools()` hosting integration and is linked from the Aspire dashboard. When
Aspire ships a dashboard plugin model, port it into a dashboard page.

This is the alternative to the browser-extension Redux DevTools integration. Observation already
goes through OpenTelemetry since task 058 (`TimeWarp.State.Telemetry`); this app is the dedicated
inspector UI on top of that, plus a home for the "control" half (time travel, commit, import/export)
if that is pursued.

Filed 2026-10-10 at Steven's request (relayed by Amina). Not launched.

## Background

Done task 058, `kanban/done/058-implement-timewarpstatetelemetry/task.md`, records this as an
out-of-scope follow-on:

- "Out of scope — follow-on tasks" (line 63): "Time-travel "control" channel: dev-only SignalR hub /
  minimal API invoking `Store.LoadStatesFromJson`/`Hydrate`. ... That path is AOT-hostile; do not pull
  it into this package."
- Line 71: "Companion devtools Blazor app (dogfooding TimeWarp.State) registered as an Aspire resource
  via a hosting integration (`AddTimeWarpStateDevTools()`), linked from the dashboard; port into a
  dashboard page when Aspire's plugin model ships."
- Line 72: "Deprecation path for the existing ReduxDevTools JS-interop feature."
- Results, line 86: "**Not in this change:** time-travel / `LoadStatesFromJson`; `AddTimeWarpStateDevTools()`
  companion app; obsoleting `UseReduxDevTools` (docs point observation at telemetry)."

## Design notes (carried over from task 097)

From `kanban/done/097-proposal-source-generator-for-state-cloning-aot-friendly/task.md` (lines 217-227):

- **(a) Recursion risk.** If the inspector manages its own UI state with TimeWarp.State, its own
  actions get captured into the action log it displays. Each displayed update produces a new captured
  action, which loops forever. Options: filter the inspector's own actions out of capture, or use a
  separate store (below).
- **(b) Separate store.** The inspector gets its own store, fully decoupled from the app, so its
  state never enters the app's logged action stream or the clone/transaction pipeline
  (`StateTransactionBehavior`). Its states must not be pulled into the app's clone registry. The 097
  generator registers only states from its own compilation, so a separate DevTools assembly/process
  keeps its states out of the app's `StateCloneRegistry`; keep it that way (no "every `IState`" rule
  that crosses assemblies).

## Requirements

- New hosting integration package/project exposing `AddTimeWarpStateDevTools()` on the Aspire
  `IDistributedApplicationBuilder`, which adds the DevTools Blazor app as a resource and surfaces a
  link to it in the Aspire dashboard.
- The DevTools app is a Blazor app that uses TimeWarp.State for its own UI state (dogfooding).
- Data source: the app's telemetry from `TimeWarp.State.Telemetry` (OTel `TimeWarp.State`
  ActivitySource) and/or a dev-only channel. Decide and record which.
- No recursion: the inspector's actions never appear in the action log it displays (separate store
  and/or capture filter, per design notes a and b).
- Inspector states are never cloned, logged or registered by the inspected app's pipeline.
- Dev-only: nothing ships in or is required by production app packages.
- Any time-travel/control channel must not reintroduce reflection into `TimeWarp.State` (see task 104).

## Open questions

- Data transport: OTLP only (observation), or also a dev-only back-channel (SignalR / minimal API) for
  control? Control depends on task 104's generated hydration (or on M1's "delete time travel" choice).
- Where the integration package lives (this repo vs a separate repo) and its package id.
- Deprecation timeline for `UseReduxDevTools` / the Redux DevTools JS interop once this exists.

## Checklist

- [ ] Design: transport, package layout, separate-store vs filter decision (record in Results before code)
- [ ] `AddTimeWarpStateDevTools()` Aspire hosting integration (resource + dashboard link)
- [ ] DevTools Blazor app: action log and state view, using its own TimeWarp.State store
- [ ] Recursion guard: inspector actions excluded from capture (test)
- [ ] Inspector states not in the app's clone registry / transaction pipeline (test)
- [ ] Sample AppHost wiring (e.g. extend `samples/04-telemetry`)
- [ ] Docs: topic page + readme; note on Redux DevTools deprecation path
- [ ] Code review

## Acceptance criteria

- An AppHost calling `AddTimeWarpStateDevTools()` starts the DevTools app as a resource, and the
  Aspire dashboard links to it.
- Dispatching actions in the sample app shows them in the DevTools app, and interacting with the
  DevTools UI adds no entries to the displayed log (no loop).
- The inspected app's `StateCloneRegistry` contains no DevTools states, and no DevTools action passes
  through the app's `StateTransactionBehavior`.
- `TimeWarp.State` gains no reflection or new IL2xxx/IL3xxx warnings.
- Build warning-free for new projects, tests green, `ganda repo audit` clean.

## Session

- Created: 2026-10-10 (Grok Bot, at Steven's request via Amina; not launched)

## Notes

- Related: 104 (remove remaining reflection; generated DevTools hydration), 109 M1 (Redux DevTools
  time travel unreachable; finish or delete).
