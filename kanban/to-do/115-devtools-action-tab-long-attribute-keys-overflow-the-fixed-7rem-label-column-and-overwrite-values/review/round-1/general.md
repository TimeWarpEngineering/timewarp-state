# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commit 98cda6ab (Inspector.razor/.css, StateTreeView.razor.css, StateDiffView.razor.css, span-ingest-endpoint-tests.cs)

## Summary

The `.facts` grid now uses `minmax(7rem, max-content)` for the label track; `dt { max-width: 16rem }` clamps each item's max-content contribution, so the track tops out at 16rem and long keys wrap via `overflow-wrap: anywhere` instead of collapsing the value track. Attribute `dt`s get a scoped `key` class for the mono font (no clash with other `.key` rules — the State/Diff `.key` rules are isolated to their own components). State/Diff keys gain `min-width: 0` + `overflow-wrap: anywhere`. The new test covers a break-free key on the detail model; the screenshot shows no overlap. Low risk.

## Issues

None.
