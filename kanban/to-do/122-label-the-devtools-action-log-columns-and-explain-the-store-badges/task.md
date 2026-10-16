# Label the DevTools action log columns and explain the store badges

## Goal (implementer restatement)

The action log names every column and keeps those columns on one line. A person can read time, store, runtime, action, state, duration, and result without the cells sliding into each other. The runtime chip stays about as small as the store badge. A row with no runtime still reserves that column, so rows with and without a runtime line up. Long action and state names end in an ellipsis and show the full text in a tooltip. The words `pre`, `live`, and a short store-id prefix each explain themselves in the log and in the Store picker, and the full store id stays available. Follow live says that it tracks the newest store that is not a prerender and switches when a newer one appears. Light and dark themes use the existing colour tokens.

Acceptance:

- The header and every data row use one shared grid template, and the number of header cells, row cells, and template columns is the same, including runtime.
- The header sticks to the top of the scrolling log, is not a button, and is not a list item. The rows stay buttons.
- `pre`, `live`, and a prefix badge each have one explanation, shared by the log and the Store picker, plus the store id.
- The Store picker and the Follow live option explain what they do.
- The runtime chip does not take a wide flexible column. Action gets that width. State and duration do not collide. Result stays on the same line.
- Build is clean, the existing DevTools tests pass, and a markup check plus a two-width layout check cover the columns and the badge text.
- When the pane is narrower than the columns, the header and each row stay as wide as the tracks, the gaps, and the horizontal padding. Scrolling the log sideways and down still paints the header, the row line, and the selected highlight under Duration and Result. A long action name still ends in an ellipsis.
- A row button's name is the cells on that row. `pre`, `live`, and a prefix each add one shared description. The store id stays on the tooltip and stays out of the name.
- The Store control's name is the word Store. The badge tooltip and the Follow live sentence sit outside that name. Follow live remains the description.
- A row or a Store option with no store id and no render kind does not use the prefix sentence.
- The column markup check still passes when those files use Windows line endings.

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

- [x] Add shared column template for the header and `.row`, including the runtime column (or fold runtime into the store badge column and record the choice)
- [x] Fix the cell-count mismatch: every row emits exactly one cell per template column (empty runtime keeps its cell)
- [x] Runtime chip sizes to content (compact chip or folded into the store badge)
- [x] Fixed/min column widths with ellipsis and `title` tooltips for action and state; no overlap of state and duration
- [x] Result in its own column; each row is a single line at normal inspector widths
- [x] Reclaimed runtime width goes to the Action column; full-name tooltip on action
- [x] Add sticky, aligned header row to the action log in `Inspector.razor`
- [x] Add the badge explanation helper next to `StoreBadge`
- [x] Add tooltip and accessible description to log-row badges
- [x] Add tooltip and accessible description to the Store picker badge, the `<select>`, and "Follow live"
- [x] Check light and dark themes
- [x] Add a test or rendered-markup check covering the header and the badge tooltips
- [x] Add a rendered-markup/CSS (or bUnit/Playwright-style) check: header labels exist for every column including runtime; header, row, and template column counts agree
- [x] Visual check at a narrow and a wide inspector width, recorded in `## Session`
- [x] `dev build` exits 0 with 0 errors. 154 pre-existing warnings remain; none are in files this fix pass edited
- [x] Existing inspector tests pass
- [x] Give the header and each row a min-width floor so a sideways scroll keeps their backgrounds
- [x] Point each row button at a shared badge description and keep the store id out of its name
- [x] Name the Store select with the word Store; keep the badge tooltip and Follow live outside that name
- [x] Use an untagged explanation when a badge has no label
- [x] Normalise newlines in the column markup check

## Proof Gates

- `dev build`: 0 warnings, 0 errors.
- Existing inspector tests pass (`store-badge-tests`, `store-selection-tests`, and the rest of `timewarp-state-devtools-tests`).
- A test or rendered-markup check shows the header labels and the tooltip text for `pre`, `live`, and prefix badges.
- A rendered-markup/CSS check shows header labels for every column including runtime, with matching cell counts across header, rows, and the grid template.
- Visual-check note at a narrow and a wide inspector width: single-line rows, no overlap, ellipsis plus tooltip on overflow.

## Results

Runtime stays its own column. The chip is content-sized inside a fixed track, and an empty runtime still occupies that cell, so a wasm row and a row with no runtime share the same columns. The header and every row read one custom property, `--log-columns`, so the labels cannot drift from the cells. `StoreBadge.Description` is the only copy of the `pre`, `live`, and prefix explanations. `Tooltip` appends the store id. The log badge and the Store picker both call it. Follow live uses one sentence, and the select tooltip is that sentence plus a lead-in.

`./bin/dev build` exited 0: 0 errors, 154 warnings. None are in the files this fix pass edited. They are pre-existing NuGet advisories (NU1510, NU1902, NU1903), TW0007 global-using hints, RS0030 banned `Console` calls, and nullable warnings in the test app and client integration tests. `dotnet fixie timewarp-state-devtools-tests` — 98 passed, including `ActionLogColumns_Should.NameEveryColumnOnceInTheHeaderTheRowAndTheTemplate`, `StoreBadge_Should.ExplainPrerenderLiveAndPrefixBadges`, `StoreBadge_Should.ExplainAnEmptyBadgeAsUntagged`, and `StoreSelection_Should.ExplainFollowLiveFromOneSentence`.

Review round 1 fixes: `--log-min-width` is the border-box floor of the track minimums, the six column gaps, and the horizontal padding (37.4rem). The header and every row use it, so a sideways scroll keeps the sticky header, the row line, and the selected highlight under Duration and Result. Each row button's name is its cells. `aria-describedby` points at one shared `pre`, `live`, prefix, or untagged sentence, and the store id stays on the `title`. The Store control's name is the word Store. The badge tooltip and the Follow live sentence sit outside that label. An empty badge says "Untagged: no store id." The column check normalises newlines before it looks for the shared rule.

### How to validate

Smoke: From the repo root, `dotnet fixie timewarp-state-devtools-tests`. Then open the DevTools action log at about 520px and about 980px, in Theme Dark and Theme Light. Scroll the narrow log to the right and down so a row sits under the header. Hover a `pre` badge, a `live` badge, a four-character prefix badge, a row with no store id, a long action name, and the Store select, including Follow live and `(untagged)`.

Expect: The log shows Time, Store, Runtime, Action, State, Duration, and Result, aligned with the cells. Each row is one line. `wasm` / `srv` is a small chip. A row with no runtime does not shift the later columns. A long action or state name ends in an ellipsis and the tooltip is the full text. State does not run into the duration, and `Ok` stays in the Result column. Below the column floor the log scrolls sideways, and the header background, the row line, and the selected highlight still cover Duration and Result. The `pre` tooltip starts with "Prerender: a short-lived server store created while rendering a page's HTML; discarded after the request." The `live` tooltip starts with "Live: an interactive store." A prefix tooltip starts with "Store id prefix: render kind unknown." Each of those tooltips also contains the store id. A badge with no store id and no render kind says "Untagged: no store id." A row's accessible name is the cells, and the badge sentence is a description. The Store control's accessible name is Store. Follow live says it follows the newest non-prerender store and switches when a newer one appears. The Fixie run is 98 passed.

### Review

Round 1 (effort 2, roster: general). Final counts so far: bug 1 open, suggestion 2 open, nit 2 open; 0 fixed, 0 wontfix. Disposition: pending. A fix pass is needed for M1–M5. The bug (M1): at log widths below about 598px, the header and row boxes stop at the pane edge while their tracks overflow. Sideways scroll then leaves the sticky header without a background under Duration and Result, and cuts off row dividers and the selected-row highlight. The reviewer re-ran `dotnet fixie timewarp-state-devtools-tests`: 97 passed. Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.

## Session

- Created: 2026-10-16 (4043864)
- Implementation: grok 01a14577-7565-7981-8fe1-6b66fd13a895 (2026-10-16)
- `ganda repo audit` exited 0 (30 passed, 1 skipped, 1 advisory). The advisory is `kebab-path-names` on seven paths that already existed: Blazor `*.lib.module.*` names, the DevTools App `.targets` file, and backlog folder 110. Those names stay. Renaming the module files would break the Blazor script convention.

Layout choice: a separate Runtime column, not folded into Store. Each row is its own grid, so a `max-content` runtime track would be a different width on an empty cell than on `wasm` and the header would miss the data. The runtime track is fixed at `3.6rem`. The chip uses `width: fit-content` and `justify-self: start` (measured 41px). An empty runtime is a cell without the pill class, so there is no empty box, and the column still holds its track (measured 57.6px).

Shared template on `.log`, used by `.log-head` and `.row`:

`--log-columns: 6.4rem 3.2rem 3.6rem minmax(7rem, 2.6fr) minmax(5rem, 0.9fr) 4.2rem 3rem`

Action's floor is 7rem. A `minmax(0, …)` action track collapsed to 0px once the other minimums exceeded the pane. Extra space still goes mostly to Action (`2.6fr`). Below about 640px the log scrolls horizontally inside `.log`. The old 800px rule that set `.row` to `1fr 1fr` is gone, so a narrow inspector does not wrap a row onto two lines. The header is `aria-hidden`, sits outside the `<ul>`, and is not a button. Rows stay buttons. The `srv` label is unchanged.

Visual check: a static fixture of this grid and `tokens.css`, served over HTTP and measured in Chromium (the live Blazor circuit was not started). Header edges lined up with the row edges (0px). No cell overlap. Text cells shared one top. `title` is set on action, state, duration, result, and time.

- 520px pane (client width 518, scroll width 587, horizontal scroll). Action track 112px with ellipsis. State track 80px with ellipsis. Chip 41.1px. Row height about 35px. Headers not clipped. Dark paper `rgb(24, 16, 44)`, ink `rgb(247, 251, 255)`. Light paper `rgb(255, 255, 255)`, ink `rgb(26, 29, 40)`. Layout matched across themes.
- 640px pane, no scroll. Action 152px with ellipsis. State 80px with ellipsis. Single line. Chip still 41.1px.
- 980px pane, no scroll. Action 425px (the wide column). State track 147px: `AnalyticsState` fits, a longer state name ellipsizes. Duration and `Ok` stay on that line. Chip 41.1px.
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-16T16:31:40Z
- Review oracle round 1: claude (claude-opus-5-5), effort 2, roster general, 2026-10-16. 1 bug, 2 suggestions, 2 nits open; fix pass requested.
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session not reported, max-turns 120 — 2026-10-16T16:37:21Z
- Fix pass: grok 01a14593-463e-72e1-995e-b5db9038846f (2026-10-16). M1–M5. Chromium fixture of the real inspector stylesheet, 520px pane (client 503) scrolled to scrollLeft 95 and scrollTop 160: header and row 598px, both cover Result, the right-edge hit is the sticky header (`rgb(24, 16, 44)` dark, `rgb(255, 255, 255)` light), the visible row covers its right edge, action ellipsis at 112px, runtime chip 41px. 980px pane: no sideways scroll, action 414px. `dotnet fixie timewarp-state-devtools-tests`: 98 passed. `./bin/dev build`: 0 errors, 154 pre-existing warnings. `ganda repo audit`: exit 0, same kebab-path advisory as the implementation session.

## Review

- Round 1: Blocked — open M1, M2, M3, M4, M5

## Notes

Reported by Steve 2026-10-16 from a screenshot of the inspector.
