# Round 1 — merged findings
**Date:** 2026-11-04
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:2083
- Description: `CanName` accepts `private protected` (`ProtectedAndInternal`) types in this assembly. The generated file class is not derived from the container, so naming them is CS0122. Swapping the old static `CanNameFromGeneratedCode`, which rejected `ProtectedAndInternal`, for `CanNameDeep` (lines 3532 and 3552) is a regression. A `private protected` ICloneable state is now added to `ICloneableStates`, and its `RootStateForTrim<…>` and registration do not compile. Field paths have the same gap, and that predates this change.
- Suggestion: Make `ProtectedAndInternal` always unnameable in `CanName`. Add regressions for a `private protected` ICloneable state and for a `private protected` nested type used as a `[GenerateClone]` member, each asserting no generated errors.
- Source: general
- Disposition notes: Fixed. `CanName` rejects `ProtectedAndInternal` in every assembly and still allows `ProtectedOrInternal` in this assembly. `Given_Private_Protected_ICloneable_State` emits no source. `Given_GenerateClone_Member_Of_Private_Protected_Type` reports `TWSG002` and does not emit the nested type. Both assert `GeneratedErrors` is empty.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1438
- Description: The comparer constructor path passes `source.Count` to any `int` parameter, and scoring prefers `int` constructors. A non-capacity `int`, such as `maxEntries` or `version`, gets `Count`. That can trigger constructor validation or bounds, even when a comparer-only constructor exists.
- Suggestion: Map `int` to `source.Count` only for a `capacity`-named parameter. Otherwise skip that constructor, or rank it below a comparer-only constructor. Add a test.
- Source: general
- Disposition notes: Fixed. An `int` receives `source.Count` only when its name contains `capacity`. Any other `int` makes that constructor ineligible, so a comparer-only constructor wins. `Given_Non_Capacity_Int_Is_Not_Passed_As_Count` covers `maxEntries`, `version`, and `initialCapacity`.

## Duplicates / conflicts

- None (single reviewer).
