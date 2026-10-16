# Round 1 — general
**Date:** 2026-10-16
**Scope reviewed:** branch `task/122-label-the-devtools-action-log-columns-and-explain` (5093b07f) vs `master` (c2c4e444): `Inspector.razor`, `Inspector.razor.css`, `store-badge.cs`, `store-selection.cs`, `action-log-columns-tests.cs`, `store-badge-tests.cs`, `store-selection-tests.cs`.

## Summary

The change does what the task asked. The header and every row read one `--log-columns` template with seven tracks. Every row emits seven `data-col` cells, and an empty runtime keeps its cell. The badge explanations live once in `StoreBadge.Description`/`Tooltip`, and Follow live is one sentence that `PickerHelp` builds on. I re-ran `dotnet fixie timewarp-state-devtools-tests`: 97 passed. One layout defect remains at the narrow widths the implementer chose to handle with horizontal scroll. The row and header boxes stay at 100% of the pane while their grid tracks overflow it, so backgrounds and borders stop short when the log scrolls sideways. The accessibility wiring works, but it makes names very long: every row's name and the select's name include the full explanation sentence and GUID.

Goal restatement vs task: it matches. The only drift is the build gate. The task says "0 warnings", and the implementer reworded the checklist item to "0 errors, 14 pre-existing NuGet warnings". Results states the deviation plainly, and the warnings are NU19xx advisories on projects this diff does not touch. That is not a misread, so it is not filed as an issue.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state-devtools/components/pages/Inspector.razor.css:156
- Description: `.log-head, .row` set `width: 100%`. The track minimums (6.4 + 3.2 + 3.6 + 7 + 5 + 4.2 + 3 rem, plus 6 × 0.6rem gaps and 1.4rem padding ≈ 37.4rem ≈ 598px) exceed that whenever the log pane is narrower. The implementer's own 520px measurement shows client 518 / scroll 587. The pane is that narrow at common viewports of about 800–1100px, where `.workspace` gives the log about 1.25/2.2 of the width. The grid tracks then overflow the grid container. The sticky header's `--twe-paper` background, the row `border-bottom`, and the `.row.selected` / `.row.init` background and inset bar all paint only on the 100% box. Scroll sideways and the Duration/Result headings have no background, so row text shows through under the sticky header. The selected-row highlight and the row dividers also end mid-row. The requirement "no overlap" fails at exactly the width this design is meant to handle.
- Suggestion: Make the header and row boxes at least as wide as their tracks. One option is a second custom property next to `--log-columns` (for example `--log-min-width`) applied as `min-width` on `.log-head, .row`. Another is to wrap the header and list in one block with `min-width: max-content`-equivalent sizing that does not defeat the action ellipsis. Do not use plain `max-content` on the grid, because the `fr` tracks would then grow to the full action text. Add a visual-check note at about 520px with the log scrolled right, with a selected row and a vertically scrolled list under the sticky header.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-state-devtools/components/pages/Inspector.razor:357
- Description: The log-row badge carries `aria-label="@badgeHelp"`, which is the full description plus `\n` plus the full store GUID. It sits inside the row `<button>`, so each row's accessible name becomes "12:00:00.000 Prerender: a short-lived server store created while rendering a page's HTML; discarded after the request. 7d189a86-… wasm TrackEventActionSet AnalyticsState 2.0 ms Ok". A screen reader reads the long sentence and a GUID on every row. The task requires that rows stay screen-reader-usable. Also, ARIA 1.2 prohibits `aria-label` on a role-less `<span>`, so support differs between browsers.
- Suggestion: Keep `title` for the hover tooltip. For assistive tech, render three shared, visually hidden description elements once (pre, live, prefix) with stable ids. Point each row `<button>` at the matching one with `aria-describedby`, or use a short visually hidden expansion such as "prerender store". Keep the GUID out of the per-row name.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-state-devtools/components/pages/Inspector.razor:481
- Description: The picker badge (with its long `aria-label`, including the GUID) and the visible `<span id="store-picker-help">` are both inside `<label class="store-select">`. The select's accessible name comes from the whole label's content: "Store", then the badge explanation and GUID, then the Follow live sentence. `aria-describedby="store-picker-help"` then announces the Follow live sentence a second time.
- Suggestion: Move the help span (and the badge, or at least its `aria-label`) out of the `<label>`, or give the `<select>` `aria-labelledby` pointing only at the "Store" text. Keep `aria-describedby` for the help.
- Status: open

### Issue 4 — Severity: nit
- File: source/timewarp-state-devtools/features/inspector/store-badge.cs:98
- Description: `Tooltip("", "")` returns `PrefixDescription` ("Store id prefix: render kind unknown."), but `Label("", "")` is empty: there is no badge and no prefix. Untagged log rows get that tooltip and aria-label on an invisible badge, and so does the "(untagged)" Store option. The text describes something that is not shown.
- Suggestion: When `Label` would be empty (store id and render both empty), return an untagged explanation such as "No store id: the span had no store tag", or omit the tooltip. Add a test case for it.
- Status: open

### Issue 5 — Severity: nit
- File: tests/timewarp-state-devtools-tests/action-log-columns-tests.cs:45
- Description: `css.ShouldContain(".log-head,\n.row")` depends on LF line endings. The repo has no `.gitattributes` eol rule, so a Windows checkout with `core.autocrlf=true` produces `\r\n` and the test fails even though the CSS is correct.
- Suggestion: Normalise `\r\n` to `\n` after `File.ReadAllText` for both the razor and the CSS text, or match the selector list with a whitespace-tolerant check.
- Status: open
