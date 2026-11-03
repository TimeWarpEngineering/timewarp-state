# Fix state-clone-planner correctness bugs (JsonIgnore, non-nameable types, missing Clone, comparers, cancellation)
## Description

Review of `source/timewarp-state-source-generator/state-clone-planner.cs` (3416 LOC) found correctness defects that cause silent data loss or uncompilable generated code. Fix each with a regression test in `tests/timewarp-state-source-generator-tests/`.

## Requirements

- `HasIgnoreAttribute` (~line 1973) matches `JsonIgnoreAttribute` by name only. Read the `Condition` argument; only `Always` (or no argument) should skip the member. `WhenWritingNull` etc. must still be cloned.
- `CanNameFromGeneratedCode` (~line 3250) ignores `IsFileLocal`, internal-from-other-assembly and type arguments. The ICloneable trim-root emission does not filter through it at all. File-local ICloneable states and closed generics with private nested type arguments emit code that does not compile.
- `IsPubliclyVisible` (~line 3211) ignores type arguments, so `Wrapper<InternalType>` gets a public extension method (CS0051).
- A `[GenerateClone]` struct that classifies as Share (unmanaged, or no copied fields) gets no Clone extension and no diagnostic. Emit the method or report a diagnostic.
- Non-exact subclasses of Dictionary, HashSet, SortedSet, SortedDictionary, SortedList are constructed then `Clear()`ed (~lines 813, 957) and never receive `source.Comparer`. Exact-type paths do.
- Nothing in Plan, the syntax scans, Report or Emit checks `CancellationToken`; `Execute` in `state-clone-source-generator.cs` does not pass it through. Thread the token.
- Unverified: `IsAutoProperty` (~line 1910) may treat a record's `EqualityContract` as an auto-property and emit an accessor for a non-existent backing field. Add a test that compiles and runs the output for a reference-assembly record.

## Checklist

- [ ] JsonIgnore Condition respected, with test
- [ ] CanName / IsPubliclyVisible honour type arguments and file-local, with tests
- [ ] Share-classified GenerateClone root emits method or diagnostic
- [ ] Collection subclass clones pass Comparer
- [ ] CancellationToken threaded through planner
- [ ] EqualityContract record case verified
- [ ] All existing tests green

## Notes

Found by a code review on 2026-11-04 of all files over 500 LOC. See sibling tasks for the decomposition and performance work on the same file; land this one first.

## Session

- Created: claude code review session (2026-11-04)
