# Round 2 — merged findings
**Date:** 2026-10-16
**Sources:** general (effort 2, roster axes: general)
**Diff:** fix delta b2bccf43 vs round-1 base 7f941cfc

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state-devtools/components/pages/Inspector.razor.css:181
- Description: Below the track floor, the header and row boxes stopped at the pane edge while their tracks overflowed.
- Source: general (round 1)
- Disposition notes: `--log-min-width: 37.4rem`. That is the track minimums, the gaps, and the padding, with border-box sizing. `.log-head, .row` use it. The test derives the value from `--log-columns`.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-devtools/components/pages/Inspector.razor:355
- Description: The row badge's `aria-label`, which held the description and the GUID, was part of each row button's accessible name.
- Source: general (round 1)
- Disposition notes: The badge keeps only `title`. The row button's `aria-describedby` points at one of four shared, visually hidden description spans.

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-devtools/components/pages/Inspector.razor:487
- Description: The Store select's accessible name included the badge sentence, the GUID, and the Follow live help.
- Source: general (round 1)
- Disposition notes: The `<label>` wrapper became a `<div>`. `<label for>` and `aria-labelledby` name the select "Store". The help stays the description.

### M4 — Severity: nit — Status: fixed
- File: source/timewarp-state-devtools/features/inspector/store-badge.cs:119
- Description: An untagged store used the prefix sentence.
- Source: general (round 1)
- Disposition notes: Added `UntaggedDescription` and `UntaggedDescriptionId`. `Tooltip` and `DescriptionElementId` use them when `Label` is empty. Tested.

### M5 — Severity: nit — Status: fixed
- File: tests/timewarp-state-devtools-tests/action-log-columns-tests.cs:41
- Description: The selector-list assertion depended on LF line endings.
- Source: general (round 1)
- Disposition notes: Both files are newline-normalised before the checks.

## New findings

None.

## Duplicates / conflicts

- None. A single reviewer.

## Verification

- `dotnet fixie timewarp-state-devtools-tests`: 98 passed (re-run by the reviewer).
