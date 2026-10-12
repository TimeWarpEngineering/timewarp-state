# DevTools Action tab: long attribute keys overflow the fixed 7rem label column and overwrite values

## Description

In the DevTools inspector (task 113, PR #637) the **Action** tab lists facts as a `<dl class="facts">`
grid. Steven's screenshot (2026-10-12) shows the two span attribute rows overlapping: the keys
`timewarp.state.action` and `timewarp.state.state_type` run past the label column and paint over
`IncrementCountActionSet.Action` / `CounterState` in the value column.

Root cause, `source/timewarp-state-devtools/components/pages/Inspector.razor.css` around line 216:

```css
.facts { display: grid; grid-template-columns: 7rem minmax(0, 1fr); gap: 0.28rem 0.75rem; }
.facts dt { margin: 0; color: var(--twe-muted); }
.facts dd { margin: 0; font-family: var(--twe-font-mono); overflow-wrap: anywhere; }
```

The label track is a fixed `7rem`; `dt` has no `overflow-wrap` / `min-width: 0`, and attribute keys
are dotted identifiers with no break opportunities, so the text overflows the track instead of
wrapping or widening it. The built-in labels (State, Status, Duration, Started, Span, Activity) fit,
which is why the first six rows look fine. `Inspector.razor` lines 299-316 render the list; attribute
rows come from `selected.Attributes` ordered by key.

Filed by Claude (cockpit) at Steven's request, 2026-10-12.

## Requirements

- Labels never overlap values. Prefer letting the label column size to content with a floor and a
  cap, e.g. `grid-template-columns: minmax(7rem, max-content) minmax(0, 1fr)`, plus
  `overflow-wrap: anywhere; min-width: 0;` on `dt` so a pathological key still wraps instead of
  overflowing. Keep the two-column rhythm; do not switch to stacked rows on desktop.
- Attribute keys render in the mono font like their values (they are identifiers), muted colour kept.
- Same check for the **State** and **Diff** tabs' key columns (`StateTreeView.razor.css`,
  `StateDiffView.razor.css`): long JSON property names must wrap, not overflow.
- Narrow layout (the existing `@media` breakpoint around line 270) still stacks cleanly.
- Add a span with a deliberately long attribute key in the ingest test fixture so the detail model
  carries it; the CSS itself is verified by screenshot (dark theme, Action tab with the two
  `timewarp.state.*` rows) attached under `review/` and linked from `## Results`.

## Checklist

- [x] `.facts` grid: content-sized label column with floor/cap; `dt` wraps
- [x] Attribute keys in mono
- [x] State and Diff key columns checked for the same overflow
- [x] Narrow breakpoint verified
- [x] Screenshot of the fixed Action tab in `review/`
- [x] `ganda repo audit` clean; devtools tests green

## Results

The Action tab label column sizes to its content between a 7rem floor and a 16rem cap. `dt` uses `min-width: 0` and `overflow-wrap: anywhere`. A bare `max-content` track grows to the longest key and collapses the value track, so the cap lives on `dt` as `max-width: 16rem` (wide enough for `timewarp.state.state_type` on one line). Attribute keys use the mono font and keep the muted colour. Built-in labels stay in the sans font.

State and Diff keys use the same wrap (`min-width: 0`, `overflow-wrap: anywhere`) so a long JSON property name stays inside the row.

At the 800px breakpoint the workspace stacks to one column. The facts list stays two columns, and the labels do not overlap the values. On a 390px viewport the page does not scroll sideways.

`SpanIngestEndpoint_Should.KeepALongAttributeKeyOnTheDetailModel` posts a span whose attribute key has no spaces and asserts the log entry keeps that key, plus `timewarp.state.action` and `timewarp.state.state_type`.

Screenshot (dark theme, Action tab): [review/action-tab-dark.png](review/action-tab-dark.png).

DevTools tests: `dotnet fixie timewarp-state-devtools-tests --no-build` — 46 passed.

### How to validate

Smoke: From the repo root, `dotnet fixie timewarp-state-devtools-tests`. Then run the DevTools app, post an increment span that carries `timewarp.state.action` and `timewarp.state.state_type`, open the inspector, choose Theme: Dark, select the increment row, and open the Action tab. Repeat with the window narrower than 800px.

Expect: `timewarp.state.action` and `timewarp.state.state_type` are muted mono text in the label column and do not paint over `IncrementCountActionSet.Action` or `CounterState`. A longer key wraps inside the label column. The value column stays visible. Under 800px the action log and the detail pane stack, and the facts list remains two columns with no overlap. The new ingest test passes.

### Review

- Rounds: 1. Effort 1, roster: general.
- Final counts: 0 bug, 0 suggestion, 0 nit (0 open, 0 fixed, 0 wontfix).
- Disposition: **clean**.
- Artifacts: [review/review-framework.md](review/review-framework.md), [review/round-1/merged.md](review/round-1/merged.md), [review/disposition.md](review/disposition.md).

## Session

- Created: cockpit (2026-10-12)
- Review: claude review oracle, effort 1, general (2026-10-12)
- Implementation: grok 01a12eee-f405-7d12-87da-f4c8cfdd06a5 (2026-10-12)

## Notes

- Related: 113 (inspector + theme), 103 (DevTools app). CSS rules: tw-blazor-css-strategy (isolation
  per component, tokens global).
- Repro: run the sample 04 AppHost, click the counter, select the increment row, open the Action tab.
