# Round 2 — general
**Date:** 2026-11-04
**Scope reviewed:** fix delta `b154d5f3..a8d0839a` (round-1 base.sha to HEAD). Product files: `source/timewarp-state-source-generator/state-clone-planner.cs` and `tests/timewarp-state-source-generator-tests/state-clone-correctness-tests.cs`. I checked it against the round-1 ledger and the fix-pass part of `## Goal (implementer restatement)`.

## Summary

The fix pass makes `CanName` reject `ProtectedAndInternal` in every assembly, and it keeps `ProtectedOrInternal` nameable only in this assembly. That matches `IsAccessible` at line 1690. `CanNameFromGeneratedCode` already delegates to `CanNameDeep`, so ICloneable trim roots pick up the same rule. Comparer construction now passes `source.Count` only to an `int` named `*capacity*`. `IsComparerConstructor` makes any other `int` ineligible, and scoring counts only capacity parameters, so a comparer-only constructor wins. ConcurrentDictionary still rejects every `int`. Three regressions cover the new behavior: a `private protected` ICloneable state, a `[GenerateClone]` member of a `private protected` nested type, and the `BoundedMap`, `VersionedSet`, and `SizedSet` constructor shapes. Each one asserts no compiler errors in the generated file. `dotnet fixie timewarp-state-source-generator-tests` passed 100 of 100.

## Prior findings re-verified

- M1 (bug): fixed. `state-clone-planner.cs:2088` rejects `ProtectedAndInternal`. The assembly check at line 2093 now covers only `Internal` and `ProtectedOrInternal`.
- M2 (suggestion): fixed. `IsCapacityParameter` (line 1488) is used for eligibility (line 1461), scoring (line 1423), and argument mapping (line 1440).

## Issues

None. The delta adds no new defects.
