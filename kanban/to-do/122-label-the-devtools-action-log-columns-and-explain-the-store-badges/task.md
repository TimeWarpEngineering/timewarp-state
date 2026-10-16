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

- A header row above the action log with one heading per column (Time, Store, Action, State, Duration, Result). It uses the same grid columns as `.row`, so headings line up with the data. Share the column template (a custom property or shared rule) so the two cannot drift. It is sticky at the top of `.log`, which scrolls (`overflow: auto`).
- The header is not a button and is not part of the row list semantics. Give it accessible roles or use `aria-hidden` plus visible text consistently. Rows stay keyboard- and screen-reader-usable.
- Each store badge gets a tooltip (`title`) and an accessible description (`aria-label` or `aria-describedby`) explaining it:
  - `pre`: "Prerender: a short-lived server store created while rendering a page's HTML; discarded after the request."
  - `live`: "Live: an interactive store."
  - Prefix badge: "Store id prefix: render kind unknown."
  - The full store id stays available (the current tooltip shows it). Put the explanation text in one place next to `StoreBadge` (a `Description(render)` helper or similar) so the log row and the Store picker share it. When two things must agree, generate one from the other.
- The Store picker gets a tooltip or short visible help (`title` / `aria-describedby`) saying what it does. The "Follow live" option is explained: it follows the newest non-prerender store and switches when a newer one appears. The `pre ...` dropdown entries are covered by the same badge explanation.
- Works in light and dark themes. Use existing `--twe-*` tokens only. No new colours, no Tailwind, no `<style>` elements in components.

## Out of Scope

- Grouping, collapsing, or hiding the many per-request prerender stores in the Store dropdown. Possible follow-up task only.

## Checklist

- [ ] Add shared column template for the header and `.row`
- [ ] Add sticky, aligned header row to the action log in `Inspector.razor`
- [ ] Add the badge explanation helper next to `StoreBadge`
- [ ] Add tooltip and accessible description to log-row badges
- [ ] Add tooltip and accessible description to the Store picker badge, the `<select>`, and "Follow live"
- [ ] Check light and dark themes
- [ ] Add a test or rendered-markup check covering the header and the badge tooltips
- [ ] `dev build` is 0 warnings and 0 errors
- [ ] Existing inspector tests pass

## Proof Gates

- `dev build`: 0 warnings, 0 errors.
- Existing inspector tests pass (`store-badge-tests`, `store-selection-tests`, and the rest of `timewarp-state-devtools-tests`).
- A test or rendered-markup check shows the header labels and the tooltip text for `pre`, `live`, and prefix badges.

## Session

- Created: 2026-10-16 (4043864)

## Notes

Reported by Steve 2026-10-16 from a screenshot of the inspector.
