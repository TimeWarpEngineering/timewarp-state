# Round 1 — general
**Date:** 2026-11-04
**Scope reviewed:** same as framework (HEAD b154d5f3 vs master)

## Summary

The change covers every requirement and the restatement matches the task. JsonIgnore now reads `Condition`. Nameability walks type arguments, file-local types and other-assembly internals. Share-classified `[GenerateClone]` value types emit an identity Clone. Collection subclasses pass `source.Comparer` through a matching constructor. The token is threaded through Plan, the syntax scans, Report and Emit. Reference-assembly record `EqualityContract` is skipped. `dotnet fixie timewarp-state-source-generator-tests` ran 97 passed on this HEAD.

One regression: replacing the old static `CanNameFromGeneratedCode` with `CanNameDeep` routes ICloneable states and DI registration through `CanName`. `CanName` treats `private protected` like internal. The generated file cannot name that accessibility. There is also one heuristic in the comparer constructor choice.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state-source-generator/state-clone-planner.cs:2083
- Description: `CanName` puts `Accessibility.ProtectedAndInternal` (`private protected`) in the internal branch, so it returns true when the type is in this assembly. A `private protected` nested type is accessible only from the containing type and its derived types. The generated `file static class TimeWarpStateClones` is neither, so naming the type is CS0122. Before this change, `RememberICloneableState` and the registration path used the old static `CanNameFromGeneratedCode`, which rejected `ProtectedAndInternal`. The diff replaces it with `CanNameDeep` → `CanName` (lines 3532 and 3552), so `public class Outer { private protected sealed class Inner : State<Inner>, ICloneable { … } }` is now added to `ICloneableStates`. It then emits `RootStateForTrim<global::Outer.Inner>()` and a service registration that do not compile. Before the change it was skipped. The field cloner path (`CanNameDeep` at 384, 1564, 1700, 1773) already had the same gap, so a `[GenerateClone]` type with a `private protected` nested member type also produces uncompilable code. That breaks the task goal "generated code names a type only when … nameable" (private or protected types excluded).
- Suggestion: In `CanName`, treat `ProtectedAndInternal` like `Private`/`Protected` (always false), and keep `ProtectedOrInternal` in the same-assembly branch. Add a regression to `Should_Not_Name_Unnameable_Types` for a `private protected` ICloneable state, and one for a `private protected` nested type reached from a `[GenerateClone]` member. Each asserts `GeneratedErrors` is empty.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1438
- Description: `TryEmitComparerConstruction` passes `source.Count` to every `int` parameter of the chosen comparer constructor, and the scoring prefers constructors that have one (+10). A subclass such as `BoundedMap(int maxEntries, IEqualityComparer<string> comparer)` or `Tagged(int version, IComparer<T> comparer)` gets `source.Count` for an argument that is not a capacity. Extra fields are copied afterwards, so stored values are overwritten, but constructor side effects remain: validation, an eviction bound, or a throw when `Count` is 0. This happens even when a comparer-only constructor exists. The old path passed typed defaults, so the change adds a new semantic guess.
- Suggestion: Pass `source.Count` only when the `int` parameter is named `capacity` (the BCL name), or when its name contains `capacity` without regard to case. For any other `int`, skip that constructor, or score it below a comparer-only constructor. Add a test case for the non-capacity `int` shape.
- Status: open

## Checked and not raised

- JsonIgnore: named `Condition` and ctor-arg forms, the enum value via `JsonIgnoreCondition.Always`, and Newtonsoft `[JsonIgnore]` without arguments all still skip.
- Share-struct promotion only runs in `Consider` for roots. Ordering against earlier non-root uses is still functional (identity), and the only cost is a method call.
- `IsPubliclyVisible` type-argument walk: roots are always closed, so type parameters never reach it.
- `IsAutoProperty` now rejects `IsImplicitlyDeclared`. `EnumerateAutoProperties` is only the metadata fallback on top of field enumeration, so source positional records still copy through their fields.
- The cancellation test covers only the entry check. The `Execute` pass-through is a one-line change, and the per-loop checks were read in the diff.
- `InternalsVisibleTo` for the test assembly on the analyzer project is harmless.
