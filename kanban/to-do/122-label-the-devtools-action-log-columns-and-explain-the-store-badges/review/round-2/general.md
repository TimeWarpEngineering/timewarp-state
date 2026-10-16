# Round 2 — general
**Date:** 2026-10-16
**Scope reviewed:** fix delta b2bccf43 vs round-1 base 7f941cfc (Inspector.razor, Inspector.razor.css, store-badge.cs, action-log-columns-tests.cs, store-badge-tests.cs)

## Summary

The fix pass covers all five round-1 findings and adds nothing beyond them. `--log-min-width: 37.4rem` is the border-box floor: the track minimums (32.4rem), six 0.6rem gaps (3.6rem), and 0.7rem padding on each side (1.4rem). `app.css` sets `* { box-sizing: border-box }`, and `rem` comes from the root font size, so the 0.82rem row font does not change the floor. The header and rows now span the overflowed tracks. Row buttons no longer get an `aria-label` from the badge. The badge sentence comes through `aria-describedby`, from one of four shared, visually hidden spans. The Store select is labelled only by "Store". Untagged badges get their own sentence. The column test normalises newlines and derives the floor from the template, so the floor cannot drift from the tracks without a test failure. I found no new defects.

## Re-verification of round-1 findings

- **M1 (bug):** fixed. `.log-head, .row` have `min-width: var(--log-min-width)` (Inspector.razor.css:181). The floor matches the computed sum. The test asserts the sum from `--log-columns`, the gap, and the padding (action-log-columns-tests.cs:47–52).
- **M2 (suggestion):** fixed. The row badge has only `title`. The row button has `aria-describedby="@StoreBadge.DescriptionElementId(...)"` (Inspector.razor:355). Four shared spans in `.log-a11y` hold the sentences, and the GUID is no longer in the accessible name.
- **M3 (suggestion):** fixed. The wrapper is now a `<div>`. `<label for="store-picker">` plus `aria-labelledby="store-picker-label"` name the select "Store". The help text and the badge sit outside the label, and `aria-describedby="store-picker-help"` stays.
- **M4 (nit):** fixed. `Tooltip` and `DescriptionElementId` return the untagged sentence and id when `Label` is empty. Tests are in store-badge-tests.cs:77.
- **M5 (nit):** fixed. `NormalizeNewlines` runs on both files before the checks.

## Verification

- `dotnet fixie timewarp-state-devtools-tests`: 98 passed (re-run by this reviewer). The run compiles the DevTools project, including the new `StoreBadge` members.
- No other source, test, or e2e file refers to `label.store-select`, the removed badge `aria-label`, or the new ids. The ids appear once in `Inspector.razor`, which renders the log and the picker once per page.

## Issues

None.
