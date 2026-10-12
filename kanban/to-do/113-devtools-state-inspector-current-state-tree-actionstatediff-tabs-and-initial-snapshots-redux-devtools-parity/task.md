# DevTools state inspector: current state tree, Action/State/Diff tabs, and initial snapshots (Redux DevTools parity)

## Description

Steven's first look at the companion DevTools app (task 103 / PR #635, endpoint fix in 111 / PR #636):
the action log works, but the right-hand pane only prints the selected span's raw JSON under the
label `state.diff`. Redux DevTools gives a **State** view: every state type in the store as a
collapsible tree, with Action / State / Diff tabs for the selected action. We need at least that.

What the channel already carries (no protocol change needed for the UI half):

- `TimeWarp.State.Telemetry` emits one span per action with `timewarp.state.state_type` and a span
  event carrying the full post-action JSON of the enclosing state: event `state.snapshot` the first
  time, `state.diff` on later changes (`telemetry-behavior.cs`, `state-snapshot-cache.cs`). Both
  events carry the complete JSON, not a patch. Snapshots exist only when the host sets
  `IncludeSnapshots` and supplies `JsonTypeInfo` (sample 04: `SampleJsonContext`).
- The DevTools ingest keeps the latest such event per span as `ActionLogEntry.SnapshotJson` /
  `SnapshotKind` (`span-ingest-endpoint.cs`), and `Inspector.razor` prints it in a `<pre>`.

Gaps:

1. No **current state tree**: nothing folds the per-action snapshots into "latest JSON per state
   type" and renders it as a tree.
2. No **State at selected action** / **Diff** against the previous snapshot of that state.
3. The label shows the raw event name (`state.diff`) instead of "State after action".
4. A state that never dispatches an action never appears, because snapshots ride on action spans.

Filed by Claude (cockpit) at Steven's request, 2026-10-12, from screenshots of DevTools next to
Redux DevTools (Tree view with per-state nodes, Action / State / Diff / Trace tabs).

## Requirements

### Inspector UI (`source/timewarp-state-devtools`)

- **State panel** listing every state type seen (and every state announced by the initial
  snapshot, below) as a top-level node, each a collapsible JSON tree (objects, arrays, scalars with
  type-coloured values, like Redux DevTools' Tree). Tabs: **Tree** / **Raw** at minimum.
- **Selected-action detail** with tabs **Action** (name, state type, status, duration, start time,
  span id, other attributes), **State** (the state tree *as of that action*: fold snapshots up to and
  including the selected sequence), **Diff** (structural JSON diff of the selected action's state vs
  the previous snapshot of the same state type: added / removed / changed, colour coded; "no change"
  when equal; "no snapshot" when the span had none). Keep the truncated marker.
- With no action selected, State shows the current tree (fold of all entries).
- Expanded/collapsed node paths persist across re-renders and new rows (store them in
  `InspectorState`; those actions stay excluded from the log like today).
- Fold / diff / tree-model logic lives in plain C# (e.g. `features/inspector/state-tree.cs`,
  `state-diff.cs`) using `System.Text.Json.Nodes` (no reflection, AOT-safe; the inspector does not
  reference `TimeWarp.State.Telemetry`). `InspectorLog` keeps a per-state latest-snapshot index so
  the fold is O(states), not O(entries), for the current view.
- Filter box also matches state type names in the State panel.
- Follow tw-blazor / tw-blazor-css-strategy (CSS isolation, no `<style>` in components).

### Initial snapshots (`source/timewarp-state-telemetry`, opt-in)

- When `IncludeSnapshots` is on, emit a `state.snapshot` for a state when it is **initialized**
  (store creation / `Initialize`), not only on its first action, so the State panel is complete on
  connect. Use the existing initialization pipeline (state-initialization pre-processor /
  `StateInitializedNotification`) and the same `JsonTypeInfo` rule (skip with the existing warning
  when none). These may be their own activities (name e.g. `CounterState.Initialized`) tagged with
  `timewarp.state.state_type`; the inspector shows them as rows of kind `init` (Redux DevTools shows
  `init` rows too) and folds them like any snapshot. No new reflection; no change to the OTLP path
  other than the extra spans.
- If this half grows beyond a small change, split it as a child task (`ganda kanban reserve
  --parent <this id>`) and ship the UI half first; the UI must still work with today's channel.

### Tests

- Unit: fold (latest per state up to a sequence), diff (add / remove / change / nested / arrays /
  equal), tree model from JSON, truncation flag preserved.
- Ingest: two state types interleaved, init rows.
- DevTools tests project (`tests/timewarp-state-devtools-tests`) stays Fixie; component rendering
  can be covered through the existing pattern there (or bUnit if already referenced; do not add a
  new test framework).
- Manual Linux check with the sample 04 AppHost: counter clicks update the CounterState node live;
  selecting an older row shows the older Count in State and the +5 in Diff. Record in `## Results`.

### Out of scope

Time travel / commit / revert / import-export (the "control" channel, task 058 follow-on), Chart
view, Trace tab, persistence of the log across DevTools restarts.

## Checklist

- [ ] `InspectorLog`: per-state latest-snapshot index + fold-up-to-sequence
- [ ] `state-tree.cs` + `state-diff.cs` (JsonNode based) with unit tests
- [ ] State panel: per-state collapsible tree, Tree / Raw, expand state persisted in `InspectorState`
- [ ] Detail: Action / State / Diff tabs; "State after action" wording; truncated marker kept
- [ ] Filter matches state type names in the State panel
- [ ] Telemetry: snapshot on state initialization (opt-in via `IncludeSnapshots`), `init` rows in DevTools
      (or child task filed and linked here)
- [ ] Ingest tests for interleaved states and init rows
- [ ] Docs: `documentation/topics/devtools.md` and the DevTools readme describe the State panel and tabs
- [ ] Manual Linux check recorded in `## Results`
- [ ] `ganda repo audit` clean; build warning-free for touched projects; tests green

## Session

- Created: cockpit (2026-10-12)

## Notes

- Related: 103 (DevTools app), 111 (Aspire endpoint fix), 058 (telemetry), 112 (Development
  container validation), 109 M1 / 104 (Redux DevTools dead path; this app is its replacement).
- Reference UI: Redux DevTools extension, Actions list on the left (`init`, action names, time),
  State tab with Tree / Chart / Raw, Action / State / Diff / Trace / Test tabs on the right.
- Snapshot JSON may be truncated at `MaxSnapshotChars` (default 16 Ki) and again at ingest
  (`MaxSnapshotLength` 64 Ki); a truncated snapshot cannot be parsed as a tree, so show Raw with
  the marker and a hint to raise `MaxSnapshotChars`.
