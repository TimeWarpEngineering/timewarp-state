# Round 1 — general
**Date:** 2026-10-11
**Scope reviewed:** same as framework

## Summary

The change replaces the count-only collection check with `SequenceEqual` on materialized snapshots. It short-circuits on reference equality, null, and `IQueryable`. All value types (with `Nullable<T>` unwrapped) and `string` now go through `CheckPrimitiveParameterChanged` (`Equals`). Risk is low. The comparison only runs when a check hook is overridden. Both acceptance criteria are covered by unit tests. One efficiency regression was found and no correctness bugs.

Checked and not raised:
- Same-instance lists mutated in place still compare as unchanged. The old count check did the same, so this is not a regression.
- Lazy non-`IQueryable` sequences are enumerated. The old `Count()` did that too.
- Value-type collections such as `ImmutableArray<T>` now take the `Equals` path. Two arrays with the same items in different backing arrays compare as changed, which can only over-render.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-state-blazor/components/timewarp-state-component.check-complex-parameter-changed.cs:217
- Description: The old `Cast<object>().Count()` was O(1) for `List<T>`, because `Enumerable.Count` uses the non-generic `ICollection.Count`. The new code copies both collections into arrays on every parameter set, even when the counts already differ.
- Suggestion: When both values are `ICollection` and their counts differ, return true before taking the snapshots. Add a test for the different-count case.
- Status: open

### Issue 2 — Severity: nit
- File: tests/timewarp-state-tests/timewarp-state-component/parameter-change-tests.cs:107
- Description: `SameItems_DoNotReportAChange` asserts `RenderReasonDetail == "... Null value change"`. That is a leftover from the initial null-to-list apply. It proves the detail was not overwritten, but it reads oddly.
- Suggestion: No change needed. The assertion is what proves "does not report a change".
- Status: open
