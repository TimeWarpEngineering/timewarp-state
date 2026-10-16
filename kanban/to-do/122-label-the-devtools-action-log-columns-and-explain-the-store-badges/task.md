# Label the DevTools action log columns and explain the store badges

## Description

The inspector's action log has no column headings, and the store badge words are unexplained. Steve reported this on 2026-10-16 from a screenshot and had to ask what "pre" meant.

Each log row has six columns: time, store badge, action name, state type, duration, result. Nothing names them. The badge shows `pre`, `live`, or a four-character store-id prefix, and nothing says what any of them mean. The Store dropdown lists entries such as `pre 7d189a86-...` with no explanation, and the "Follow live" option is unexplained.

Where it lives:

- Row markup: `source/timewarp-state-devtools/components/pages/Inspector.razor`, the `<section class="log">` block (`<ul>` of `<button class="row">` rows). The badge is `<span class="store-badge">` with `title="@entry.StoreId"`. The Store picker is further down: a `<span class="store-badge">` plus `<select data-qa="store">` with the `FollowOption` ("Follow live") option.
- Badge text: `source/timewarp-state-devtools/features/inspector/store-badge.cs` (`PrerenderLabel` "pre", `InteractiveLabel` "live", `PrefixLength` 4).
- Follow live: `source/timewarp-state-devtools/features/inspector/store-selection.cs`. `Effective` resolves Follow live to the newest non-prerender store.
- Styles: `source/timewarp-state-devtools/components/pages/Inspector.razor.css` (`.log`, `.row` grid `7.2rem 3.4rem minmax(0,1.4fr) minmax(0,0.8fr) 5.5rem 4.5rem`, `.store-badge`, `--twe-*` tokens).
- Tests: `tests/timewarp-state-devtools-tests/store-badge-tests.cs`, `store-selection-tests.cs`.

## Requirements

- A header row above the action log with one heading per column (Time, Store, Action, State, Duration, Result, plus Runtime unless folded into Store; see the amendment). It uses the same grid columns as `.row`, so headings line up with the data. Share the column template (a custom property or shared rule) so the two cannot drift. It is sticky at the top of `.log`, which scrolls (`overflow: auto`).
- The header is not a button and is not part of the row list semantics. Give it accessible roles or use `aria-hidden` plus visible text consistently. Rows stay keyboard- and screen-reader-usable.
- Each store badge gets a tooltip (`title`) and an accessible description (`aria-label` or `aria-describedby`) explaining it:
  - `pre`: "Prerender: a short-lived server store created while rendering a page's HTML; discarded after the request."
  - `live`: "Live: an interactive store."
  - Prefix badge: "Store id prefix: render kind unknown."
  - The full store id stays available (the current tooltip shows it). Put the explanation text in one place next to `StoreBadge` (a `Description(render)` helper or similar) so the log row and the Store picker share it. When two things must agree, generate one from the other.
- The Store picker gets a tooltip or short visible help (`title` / `aria-describedby`) saying what it does. The "Follow live" option is explained: it follows the newest non-prerender store and switches when a newer one appears. The `pre ...` dropdown entries are covered by the same badge explanation.
- Works in light and dark themes. Use existing `--twe-*` tokens only. No new colours, no Tailwind, no `<style>` elements in components.

## Amendment 2026-10-16 (Steve, from a screenshot of live WASM stores)

Task 121 added a runtime chip (`<span class="runtime-badge" data-qa="runtime">`, values `wasm` / `server`, from span tag `timewarp.state.runtime`) to each row in `Inspector.razor`. It is rendered conditionally as a seventh child of `.row`, but `.row` still declares six grid columns (`7.2rem 3.4rem minmax(0,1.4fr) minmax(0,0.8fr) 5.5rem 4.5rem`). Root cause of the defects below: every row now has seven cells in a six-column grid, so cells shift one column right (the chip sits in the wide Action column, the action name in the State column, and so on), and the last cell wraps to an implicit second line. Rows without a runtime label (empty value) shift differently, so columns also disagree between rows.

Observed defects and requirements:

- Still no column headers. Already in scope; reaffirmed. Every column gets a header label, including runtime.
- The runtime column (values `wasm` / `server`, a short fixed vocabulary) renders as a wide pill/box taking far more width than its content. It must size to content: a compact chip like the store badge, or fold runtime into the store badge column. The implementer chooses and records which in `## Session`. Whichever is chosen, an empty runtime still occupies its cell (render an empty placeholder cell or fold in) so rows never shift.
- The state name collides with the duration (`AnalyticsState2.0 ms` renders with no gap). Columns need fixed or min widths, with ellipsis (`overflow: hidden; text-overflow: ellipsis; white-space: nowrap`) and a `title` tooltip showing the full text on overflow. Cells never overlap.
- The result (`Ok`) wraps onto a second line under the time. Each log entry is a single line at normal inspector widths, with the result in its own column.
- Action names truncate early (`TrackEventActionSet...`) because of the wasted runtime width. Reclaimed width goes to the action column, which carries a full-name tooltip.
- The header row and data rows share one grid column template (already a requirement). The runtime column is included in that template, with a header label. The cell count of the header and of every row must equal the template's column count.

Added proof item: a rendered-markup/CSS test (or bUnit/Playwright-style check) that the header labels exist for every column including runtime, and that header cell count equals row cell count equals template column count. Add a visual-check note, recorded in `## Session`, at a narrow and a wide inspector width: single-line rows, no overlap, ellipsis plus tooltip on overflow, headers aligned.

## Out of Scope

- Grouping, collapsing, or hiding the many per-request prerender stores in the Store dropdown. Possible follow-up task only.

## Checklist

- [ ] Add shared column template for the header and `.row`, including the runtime column (or fold runtime into the store badge column and record the choice)
- [ ] Fix the cell-count mismatch: every row emits exactly one cell per template column (empty runtime keeps its cell)
- [ ] Runtime chip sizes to content (compact chip or folded into the store badge)
- [ ] Fixed/min column widths with ellipsis and `title` tooltips for action and state; no overlap of state and duration
- [ ] Result in its own column; each row is a single line at normal inspector widths
- [ ] Reclaimed runtime width goes to the Action column; full-name tooltip on action
- [ ] Add sticky, aligned header row to the action log in `Inspector.razor`
- [ ] Add the badge explanation helper next to `StoreBadge`
- [ ] Add tooltip and accessible description to log-row badges
- [ ] Add tooltip and accessible description to the Store picker badge, the `<select>`, and "Follow live"
- [ ] Check light and dark themes
- [ ] Add a test or rendered-markup check covering the header and the badge tooltips
- [ ] Add a rendered-markup/CSS (or bUnit/Playwright-style) check: header labels exist for every column including runtime; header, row, and template column counts agree
- [ ] Visual check at a narrow and a wide inspector width, recorded in `## Session`
- [ ] `dev build` is 0 warnings and 0 errors
- [ ] Existing inspector tests pass

## Proof Gates

- `dev build`: 0 warnings, 0 errors.
- Existing inspector tests pass (`store-badge-tests`, `store-selection-tests`, and the rest of `timewarp-state-devtools-tests`).
- A test or rendered-markup check shows the header labels and the tooltip text for `pre`, `live`, and prefix badges.
- A rendered-markup/CSS check shows header labels for every column including runtime, with matching cell counts across header, rows, and the grid template.
- Visual-check note at a narrow and a wide inspector width: single-line rows, no overlap, ellipsis plus tooltip on overflow.

## Session

- Created: 2026-10-16 (4043864)

## Notes

Reported by Steve 2026-10-16 from a screenshot of the inspector.
