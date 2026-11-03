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

- [x] JsonIgnore Condition respected, with test
- [x] CanName / IsPubliclyVisible honour type arguments and file-local, with tests
- [x] Share-classified GenerateClone root emits method or diagnostic
- [x] Collection subclass clones pass Comparer
- [x] CancellationToken threaded through planner
- [x] EqualityContract record case verified
- [x] All existing tests green

## Results

The clone planner no longer drops members or emits signatures the compiler cannot name.

- `[JsonIgnore]` skips a member only when `Condition` is `Always` or omitted. `WhenWritingNull`, `WhenWritingDefault`, and `Never` are still copied. The `Always` value is read from `System.Text.Json.Serialization.JsonIgnoreCondition` when that enum is in the compilation, otherwise the integer 1. Newtonsoft `JsonIgnore`, `IgnoreDataMember`, and `NonSerialized` still always skip.
- A type is nameable from generated code only when the type, its containers, and every type argument are nameable. File-local types, private and protected types, and internals from another assembly are excluded. ICloneable trim roots and DI service registration use that same check. `IsPubliclyVisible` walks type arguments, so `Wrapper<InternalType>` gets an `internal` extension and `Wrapper<PublicType>` stays `public`.
- A `[GenerateClone]` value type that classifies as Share (unmanaged, or no copied fields) is promoted to an identity clone (`return source;`). Reference types that are Share are unchanged. Non-root unmanaged fields stay Share.
- A subclass of `Dictionary`, `HashSet`, `SortedSet`, `SortedDictionary`, `SortedList`, or `ConcurrentDictionary` is constructed with `source.Comparer` when one of its constructors accepts that comparer (and, when the chosen constructor also takes an `int`, `source.Count`). A subclass that only has a parameterless constructor still uses that constructor. C# does not inherit constructors, so comparer cannot be preserved in that shape. The existing parameterless `CollectionSubclass` tests stay green.
- `Execute` passes `sourceContext.CancellationToken` into `Plan`. The planner checks it at the start of planning, in both syntax scans, between phases, and inside report and emit loops. A cancelled token throws `OperationCanceledException` before the source-production context is touched.
- A record `EqualityContract` property (get-only `System.Type`, protected or private, implicit or compiler-generated getter) is not treated as an auto-property and is not rejected as a reference-assembly non-auto-property. Positional record properties still copy through their backing fields. The regression compiles an unsealed record as a reference assembly and runs the generated clone against the implementation image.

### Key decisions

- Share-classified `[GenerateClone]` structs emit an identity method. They do not get a diagnostic.
- Other-assembly internals stay unnameable even when `InternalsVisibleTo` would let user code name them.
- A hand-written `protected Type EqualityContract { get; }` on a record is skipped by the same predicate. That shape is rare.

### Files

- `source/timewarp-state-source-generator/state-clone-planner.cs`
- `source/timewarp-state-source-generator/state-clone-source-generator.cs`
- `source/timewarp-state-source-generator/timewarp-state-source-generator.csproj` (`InternalsVisibleTo` the test assembly, so the cancellation test can call `Plan`)
- `tests/timewarp-state-source-generator-tests/state-clone-correctness-tests.cs`
- `tests/timewarp-state-source-generator-tests/state-clone-generator-test-driver.cs` (`LoadReferenceClone`)
- `tests/timewarp-state-source-generator-tests/global-usings.cs`

### Tests

- `dotnet fixie timewarp-state-source-generator-tests`: 97 passed (2026-11-04).
- `./bin/dev test`: exit 0 (analyzer, source generator, state, plus, telemetry, devtools, devtools isolation, client integration, architecture 7 passed / 1 skipped).

### How to validate

**Smoke**

From the repo root:

```bash
dotnet fixie timewarp-state-source-generator-tests --tests 'StateCloneSourceGenerator_.Should_*'
./bin/dev test
```

The new cases live in `tests/timewarp-state-source-generator-tests/state-clone-correctness-tests.cs`:

- `Should_Respect_JsonIgnore_Condition`
- `Should_Not_Name_Unnameable_Types`
- `Should_Keep_Internal_Extension_For_Internal_Type_Argument`
- `Should_Emit_Identity_Clone_For_Shared_Struct`
- `Should_Pass_Comparer_To_Collection_Subclass`
- `Should_Stop_When_Planning_Is_Cancelled`
- `Should_Clone_Reference_Assembly_Record`

**Expect**

- JsonIgnore: generated source has no backing field for a bare `[JsonIgnore]` or `Condition = Always`, and it does copy `WhenWritingNull`, `WhenWritingDefault`, and `Never`.
- Naming: file-local `ICloneable` emits no source and no `TWSG002`. `GenerateClone` closed over a private nested type argument reports `TWSG002` and does not emit that type name. `Wrapper<Secret>` (internal `Secret`) emits `internal static`, and `Wrapper<Visible>` emits `public static`.
- Share struct: both the unmanaged struct and the string auto-property struct emit `Clone` with `return source;`.
- Comparer: generated construction is `new(source.Count, source.Comparer)` or `new(source.Comparer)` for the subclasses that declare those constructors.
- Cancellation: `Plan(default, new CancellationToken(true))` throws `OperationCanceledException`.
- Record: the loaded clone of `Foreign.Dto("ada", 3)` is a different instance with `Name == "ada"` and `Count == 3`. Generated source contains `<Name>k__BackingField` and `<Count>k__BackingField` and does not contain `EqualityContract`.
- `./bin/dev test` prints `Tests completed successfully!`.

**Not in scope:** a collection subclass whose only constructor is parameterless cannot receive `source.Comparer`. Sibling tasks own decomposition and performance of the same planner file.

## Notes

Found by a code review on 2026-11-04 of all files over 500 LOC. See sibling tasks for the decomposition and performance work on the same file; land this one first.

## Goal (implementer restatement)

The clone planner must not drop data or emit code that fails to compile. Each case below gets a regression test under `tests/timewarp-state-source-generator-tests/`, and the existing generator and clone tests stay green.

- `[JsonIgnore]` skips a member only when its condition is `Always` or omitted. `WhenWritingNull`, `WhenWritingDefault`, and `Never` still clone.
- Generated code names a type only when the type, its containers, and its type arguments are nameable from another file in this assembly. File-local types, private or protected types, and internals from another assembly are excluded. That same check applies to ICloneable trim roots. A public extension method is emitted only when the type and its type arguments are publicly visible, so `Wrapper<InternalType>` stays internal.
- A `[GenerateClone]` struct that needs no field copy (unmanaged, or only shared fields) still gets a Clone extension that returns the value. It does not disappear without a diagnostic.
- A subclass of `Dictionary`, `HashSet`, `SortedSet`, `SortedDictionary`, or `SortedList` is constructed with `source.Comparer` when one of its constructors accepts that comparer.
- Planning, both syntax scans, diagnostics, and emit observe the generator `CancellationToken` passed from `Execute`.
- A record seen through a reference assembly is not cloned through an `EqualityContract` backing field that does not exist. A test compiles that shape and runs the clone.

## Session

- Created: claude code review session (2026-11-04)
- Implementation: grok task-work (2026-11-04)
- Audit: `ganda repo audit --fix` pinned TimeWarp.Nuru and TimeWarp.Nuru.DevCli to 4.0.0-beta.11, bumped TimeWarp.Amuru and TimeWarp.Amuru.Tools from 2.0.0 to 3.0.0-beta.1, and added `bin/dev`, `bin/dev.dbg`, and `bin/dev.exe` to `.gitignore`. Advisory warnings left in place: task 098 has no usable worktree, and seven kebab-path names are Blazor `*.lib.module.*` filenames, a `.targets` file, and backlog task 110's folder.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1a2d6-7f64-7062-8cb4-5ababff8da4a, max-turns 500, Done — 2026-11-03T19:57:36Z
