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

- [x] Design: transport, package layout, separate-store vs filter decision (record in Results before code)
- [x] `AddTimeWarpStateDevTools()` Aspire hosting integration (resource + dashboard link)
- [x] DevTools Blazor app: action log and state view, using its own TimeWarp.State store
- [x] Recursion guard: inspector actions excluded from capture (test)
- [x] Inspector states not in the app's clone registry / transaction pipeline (test)
- [x] Sample AppHost wiring (e.g. extend `samples/04-telemetry`)
- [x] Docs: topic page + readme; note on Redux DevTools deprecation path
- [x] Code review
- [x] Isolation tests restore in the pre-pack build filter (no sample PackageReference)

## Acceptance criteria

- An AppHost calling `AddTimeWarpStateDevTools()` starts the DevTools app as a resource, and the
  Aspire dashboard links to it.
- Dispatching actions in the sample app shows them in the DevTools app, and interacting with the
  DevTools UI adds no entries to the displayed log (no loop).
- The inspected app's `StateCloneRegistry` contains no DevTools states, and no DevTools action passes
  through the app's `StateTransactionBehavior`.
- `TimeWarp.State` gains no reflection or new IL2xxx/IL3xxx warnings.
- Build warning-free for new projects, tests green, `ganda repo audit` clean.

## Results

### Design (recorded before code)

**Transport.** Observation only, from the `TimeWarp.State` ActivitySource (`TimeWarp.State.Telemetry`). The pinned OpenTelemetry exporter (1.19.1) speaks gRPC and HTTP/protobuf, so DevTools does not embed a protobuf decoder and is not a general OTLP backend. `AddTimeWarpStateDevToolsExporter()` copies completed `TimeWarp.State` activities (name, tags, snapshot or diff events, status, duration) to `POST /ingest/spans` as a small JSON document. The AppHost sets `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` to the DevTools HTTP endpoint. No control channel: time-travel, commit, and import/export stay out (task 104 / 109) and `TimeWarp.State` gains no reflection.

**Packages (this repo).**

- `TimeWarp.State.DevTools.Export` — dev-only exporter. No `IState` types, so the inspected app can reference it without registering inspector states.
- `TimeWarp.State.DevTools` — Blazor Server app. Not packed. Not referenced by production packages or by the inspected app.
- `TimeWarp.State.DevTools.Hosting` — `AddTimeWarpStateDevTools()` and `WithTimeWarpStateDevTools()`. Finds `source/timewarp-state-devtools/timewarp-state-devtools.csproj` by walking up from the AppHost, or `TIMEWARP_STATE_DEVTOOLS_PROJECT`. The HTTP endpoint is labeled "TimeWarp.State DevTools" on the Aspire dashboard.

**Separate store, plus a capture filter.** The inspector is another process with its own store. Its states are registered only by its own compilation's clone generator. The inspected app does not reference the DevTools assembly, so those states are not in its `StateCloneRegistry` and its actions are not woven into the app's `StateTransactionBehavior`. `InspectorCapture` also drops `timewarp.state.state_type=InspectorState` and `timewarp.state.inspector=true` on the exporter and on ingest, so a miswired export cannot loop. The DevTools host does not reference `TimeWarp.State.Telemetry` and does not call the exporter. UI state (filter, selection, pause, revision) is `InspectorState`. The span list is a process-wide mailbox (`InspectorLog`) because ingest is an HTTP request scope and the Blazor circuit is another scope; the circuit copies the mailbox into a re-render by dispatching `NoteLogChanged`.

**Redux DevTools.** Observation moves to telemetry plus this app. `UseReduxDevTools` stays for time-travel, commit, and import/export until generated hydration (task 104) or that surface is removed (task 109). This change does not obsolete it.

### Implementation

`TimeWarp.State.DevTools.Export` posts captured `TimeWarp.State` activities to `POST /ingest/spans`. `TimeWarp.State.DevTools.Hosting` adds the Blazor project as resource `timewarp-state-devtools`, labels endpoint `http` "TimeWarp.State DevTools", and `WithTimeWarpStateDevTools` publishes `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` and waits for the resource. `samples/04-telemetry` wires both. The DevTools app is its own store. `InspectorCapture` drops inspector spans on export and on ingest. The isolation tests reference `tests/timewarp-state-devtools-inspected-app` and the exporter, not the DevTools app and not the telemetry sample. That fixture project-references `TimeWarp.State`, the clone generator, and the mediator generator, so the pre-pack build filter restores without a packed `TimeWarp.State` package. `InspectorState` is absent from that process's clone registry, and the fixture sender has no `Inspector` dispatch method. The sample stays on PackageReferences and is still omitted from `artifacts/timewarp-state.build.slnf`. `tools/dev-cli/endpoints/workflow-command.cs` runs assert-version-ssot, clean, build, test, e2e, pack, then verify-samples.

The DevTools host calls `UseReduxDevTools` because referencing `TimeWarp.State.Blazor` links `CommitHandler`, and Development validates that constructor. The Redux DevTools component is not rendered, so `InitAsync` does not run. `MapStaticAssets` serves `_framework/blazor.web.js`. `StaticWebAssetsLoader.UseStaticWebAssets` attaches the manifest outside Development, where `dotnet run` otherwise looks for that script under `wwwroot`.

`TimeWarp.State` was not edited. Export and Hosting are the new packable packages, at the repo version `12.0.0-beta.11`. The Blazor app is not packed.

### How to validate

**Smoke:** `dotnet fixie timewarp-state-devtools-tests` and `dotnet fixie timewarp-state-devtools-isolation-tests`. Restore `artifacts/timewarp-state.build.slnf` (the filter `dev build` writes, samples omitted) with nuget.org as the only package source. From `samples/04-telemetry/apphost`, run the AppHost, open the Aspire dashboard, and follow the resource link "TimeWarp.State DevTools". Increment the sample counter. In DevTools, select the row, type in Filter, press Pause, increment again, press Resume, then Clear.

**Expect:** DevTools tests: 29 passed. Isolation tests: 4 passed. The nuget.org-only restore of the filter succeeds, and no project in that filter lists a `TimeWarp.State`, `TimeWarp.State.Blazor`, or `TimeWarp.State.Telemetry` package. The dashboard resource is `timewarp-state-devtools` and the link text is "TimeWarp.State DevTools". Counter actions appear as rows; with the endpoint set, the selected row shows `state.snapshot` JSON. Filter, pause, resume, and clear change the inspector and add no rows named for those actions. A span posted while paused stays off the list until Resume. The isolation assembly's clone registry contains `CounterState` and no `InspectorState`, and the inspected-app sender has no `Inspector` dispatch method.

Browser check on 2026-10-11 against `http://127.0.0.1:5288` (Production, so the static-asset path is the one Aspire uses when the environment is not Development): `blazor.web.js` returned 200, the circuit connected, a posted `IncrementCount` span rendered with snapshot `{"Count":4}`, an `InspectorState` span in the same body did not render, Filter `zzz` hid the row and `count` restored it, Pause held a second ingest off the list, Clear emptied the list, and the console had no errors at 390×844. The Aspire dashboard click itself was not exercised here; the hosting test invokes the URL callback and the environment callback without starting DCP.

### Review disposition

- **Rounds:** 3. **Effort:** 3 (by-diff: 2893 lines in rounds 1–2, 3842 lines in round 3). **Roster:** general (Claude Sonnet subagent). The orchestrator was the review oracle (Claude Opus 5.5, headless `ganda task work`).
- **Round 3:** re-review of the PR #635 NU1102 fix (`fdb92fda..03d4d931`, the inspected-app fixture). No new findings. Isolation tests: 4 passed.
- **Final counts (round 3):** bug 3 fixed, suggestion 5 fixed, nit 3 fixed. 0 open, 0 wontfix.
- **Disposition:** `clean`.
- **Main fixes:**
  - Ingest now tolerates null fields, rejects non-JSON content with 415, enforces 1 MiB even for chunked bodies (413), and caps snapshots at 64 Ki characters.
  - The exporter doc now matches its throw on a bad endpoint, and its catch is wider.
  - `InspectorLog` notifies every circuit when pause or clear changes, and the toggle reads the shared flag.
  - The page computes visible entries once per render.
  - Hosting no longer references Export or OpenTelemetry, and the docs say the app is not packed.
  - New `InspectorLog` and ingest tests.
- **Remaining:** the isolation test still reads the private `StateCloneRegistry.Cloners` field by reflection. Adding an accessor to `TimeWarp.State` is outside this task.
- **Artifacts:** `review/review-framework.md`, `review/round-1/{general,merged}.md`, `review/round-2/{general,merged}.md`, `review/round-3/{general,merged}.md`, `review/disposition.md`.

## Session

- CI restore fix (2026-10-11). PR #635 run 38115759236 failed at build with NU1102 because the isolation tests project-referenced `samples/04-telemetry`, whose PackageReferences are `TimeWarp.State` 12.0.0-beta.11. The fixture `tests/timewarp-state-devtools-inspected-app` replaces that reference and project-references source. Pipeline order in `workflow-command.cs` is assert-version-ssot → clean → build → test → e2e → pack → verify-samples. A nuget.org-only restore of the 24-project build filter succeeded, with no `TimeWarp.State*` package in those assets. Isolation tests: 4 passed.

- Created: 2026-10-10 (Grok Bot, at Steven's request via Amina; not launched)
- Implementation: 2026-10-11 (implementer)
- Review: 2026-10-11 (review oracle, Claude Opus 5.5; general reviewer subagents; 2 rounds, clean)
- Review round 3: 2026-10-11 (review oracle, Claude Opus 5.5, headless; re-review of the NU1102 fix delta; clean)
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-11T05:43:31Z
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-11T07:52:17Z

## Notes

- Related: 104 (remove remaining reflection; generated DevTools hydration), 109 M1 (Redux DevTools
  time travel unreachable; finish or delete).
