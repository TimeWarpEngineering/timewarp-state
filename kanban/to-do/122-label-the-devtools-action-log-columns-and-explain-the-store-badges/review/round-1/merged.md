# Round 1 — merged findings
**Date:** 2026-10-16
**Sources:** general (effort 2, roster axes: general)
**Diff:** 5093b07f vs master c2c4e444

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 2 | 0 | 0 |
| nit | 2 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-state-devtools/components/pages/Inspector.razor.css:156
- Description: `.log-head, .row` are `width: 100%`, but the track minimums total about 37.4rem (about 598px). Below that width the grid tracks overflow the box. The sticky header background, the row dividers, and the selected/init row backgrounds then stop at the pane edge when the log scrolls sideways. Rows show through under the overflowed Duration/Result headings. This happens at viewports of roughly 800–1100px.
- Suggestion: Give `.log-head, .row` a `min-width` equal to the summed track minimums (a sibling custom property next to `--log-columns`), or use an equivalent wrapper. Keep the action ellipsis working. Re-run the visual check at about 520px with the log scrolled right and vertically scrolled under the header.
- Source: general
- Disposition notes:

### M2 — Severity: suggestion — Status: open
- File: source/timewarp-state-devtools/components/pages/Inspector.razor:357
- Description: The row badge's `aria-label` (the description plus the full GUID) becomes part of every row button's accessible name. That is very verbose for screen-reader users, and `aria-label` on a role-less span is not reliably supported.
- Suggestion: Keep `title`. Use `aria-describedby` on the row button pointing at shared hidden pre/live/prefix descriptions, or a short visually hidden expansion. Leave the GUID out of the name.
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: source/timewarp-state-devtools/components/pages/Inspector.razor:481
- Description: The help span and the badge `aria-label` sit inside the `<label>`, so the select's accessible name includes the badge sentence, the GUID, and the Follow live sentence. `aria-describedby` then repeats the help.
- Suggestion: Move the help (and the badge label) out of the `<label>`, or use `aria-labelledby` on the select pointing only at "Store".
- Source: general
- Disposition notes:

### M4 — Severity: nit — Status: open
- File: source/timewarp-state-devtools/features/inspector/store-badge.cs:98
- Description: For an untagged store (empty id and empty render), the label is empty but the tooltip says "Store id prefix: render kind unknown." That applies to log rows and to the "(untagged)" option.
- Suggestion: Return an untagged explanation or no tooltip when `Label` is empty, and test it.
- Source: general
- Disposition notes:

### M5 — Severity: nit — Status: open
- File: tests/timewarp-state-devtools-tests/action-log-columns-tests.cs:45
- Description: The `".log-head,\n.row"` assertion depends on LF line endings, and the repo has no `.gitattributes` eol rule.
- Suggestion: Normalise `\r\n` after reading the files.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. A single reviewer.

## Verification

- `dotnet fixie timewarp-state-devtools-tests`: 97 passed (re-run by the reviewer).
- No other test or e2e file references `runtime-badge`, `log-head`, or the row/runtime `data-qa` selectors, so the markup change does not break other suites.
