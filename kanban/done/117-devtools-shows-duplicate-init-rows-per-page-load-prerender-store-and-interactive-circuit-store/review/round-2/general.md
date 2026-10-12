# Round 2 — general
**Date:** 2026-10-12
**Scope reviewed:** fix delta 9548c05e + re-verification of round-1 M1–M8

## Summary
All eight round-1 findings are fixed in the code, not only marked fixed. A nullable pin (`null` = follow, `""` = untagged bucket) now drives `StoreSelection.Effective`. `StoreSelection.Default` skips prerender stores, and the selector offers **Follow live** and **(untagged)** options. Store matching uses a prefix. Badge hues are tokens, and the store list is computed once per render. Both test projects pass (devtools 59/59, core 116 passed / 1 skipped), and the new StoreSelection, InspectorLog, StoreBadge and StateRenderKindObserver tests are discovered. I found no new correctness defects. There is one test-coverage nit.

## Prior findings
| ID | Status | Note |
|----|--------|------|
| M1 | fixed | `StoreSelection.Default` returns the newest store whose render is not `prerender` and falls back to `stores[0]`. `FoldCurrent` (inspector-log.cs:85) and the page (`StoreSelection.Effective`) both use it. Covered by `KeepTheLiveStoreCurrentWhenALaterPrerenderStoreArrives` and `FallBackToTheNewestStoreWhenEveryStoreIsAPrerender`. |
| M2 | fixed | The selector's first option is `follow:` ("Follow live"). It dispatches the new `FollowDefaultStore` action, which sets `SelectedStoreId = null`. `StoreSelectValue` shows Follow live whenever the pin is null or no longer listed. |
| M3 | fixed | `SelectedStoreId` is now `string?`: null means no pin and `""` pins the untagged bucket. `IsListed` is false for null. The untagged bucket is listed as `untagged:` → "(untagged)", and `OnStore` maps it back to `""`. Clicking an untagged row pins `""`, and `Effective` returns `""`, so `stateAfterAction` holds. Covered by `TellTheUntaggedBucketApartFromNoPin`. |
| M4 | fixed | `StoreBadge.Matches` uses `StartsWith` on the id and the label. The State panel shows "No state matches the filter." when `folded` is non-empty and the filter empties it. Covered by `MatchOnlyTheStartOfAStoreIdOrBadge`. |
| M5 | fixed | Tests were added for per-store `FoldThrough` with interleaving, a pin that holds when a newer store arrives, Clear resetting the pin to null (inspector-loop-tests), and the three `StateRenderKindObserver` paths. |
| M6 | fixed | devtools.md, readme.md and the store-badge.cs Design region now say "usually distinct", about one pair in eight. |
| M7 | fixed | `--twe-store-ink` and `--twe-store-hue-0..7` are in tokens.css, with light values in both `[data-theme="light"]` and the `prefers-color-scheme: light` block. Inspector.razor.css uses only `var(...)`. Light hue 6 is now `#c3c7d4`, not near-white. |
| M8 | fixed | The render block calls `InspectorLog.ListStores()` once. `RenderOf` and `VisibleStates` take that list through `StoreSelection.RenderOf`, and `SelectedEntry()` is computed once and passed to `FoldedStates`. |

## Issues
### Issue 1 — Severity: nit
- File: tests/timewarp-state-devtools-tests/store-selection-tests.cs:29
- Description: `StoreSelection.Default` has a deliberate branch where a store with no render tag counts as live (store-selection.cs Design: "A store with no render tag counts as live"). No test pins it down. The existing tests use only interactive or prerender stores, or an untagged bucket that is older than the live store, so in each of them the newest non-prerender store wins on recency alone. A change that made Default skip render-less stores, or prefer only `interactive`, would still pass every test.
- Suggestion: Add a case with an older render-less store and a newer prerender store. Assert that `Default` returns the render-less store and that `Effective(stores, null)` does too.
- Status: open

## Checked, no issue
- Nullable handling: `StoreSelectValue` uses `pinned!` only after `IsListed` returns true, which requires non-null. `Effective` uses the same guard. Every `SelectedStoreId` reset (Clear, state Initialize, FollowDefaultStore) uses null.
- The selector is still rendered only when a tagged store exists. With untagged spans only, it stays hidden, as before.
- Thread-safety: `ListStores` and the later `FoldStore`/`FoldThrough` calls each take the lock separately. A Clear between them gives an empty fold for that render only, and the `Changed` event triggers the next render. This is harmless.
- Sentinel option values (`follow:`, `untagged:`) cannot collide with a Guid store id.
