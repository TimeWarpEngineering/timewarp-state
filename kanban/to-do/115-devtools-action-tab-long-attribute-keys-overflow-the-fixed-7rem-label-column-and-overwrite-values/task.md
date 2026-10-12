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

- [ ] `.facts` grid: content-sized label column with floor/cap; `dt` wraps
- [ ] Attribute keys in mono
- [ ] State and Diff key columns checked for the same overflow
- [ ] Narrow breakpoint verified
- [ ] Screenshot of the fixed Action tab in `review/`
- [ ] `ganda repo audit` clean; devtools tests green

## Session

- Created: cockpit (2026-10-12)

## Notes

- Related: 113 (inspector + theme), 103 (DevTools app). CSS rules: tw-blazor-css-strategy (isolation
  per component, tokens global).
- Repro: run the sample 04 AppHost, click the counter, select the increment row, open the Action tab.
