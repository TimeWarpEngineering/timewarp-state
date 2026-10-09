# Round 2 — general
**Date:** 2026-10-10
**Scope reviewed:** fix delta df60c9c9..HEAD plus re-verification of round-1 M1–M14

## Summary
All 14 round-1 findings are fixed. I rebuilt and re-ran round-1 probes p1–p5 against HEAD: they compile and print correct results. The generator tests (51/51) and timewarp-state-tests (94 passed, 1 skipped) pass. test-app-client, timewarp-state-plus-tests and client-integration-tests build. TimeWarp.State builds with `IsAotCompatible=true` and no IL warnings.

The fix delta introduces two runtime-throw gaps where the build should report an error or clone correctly (N1, N2), and one new runtime regression in the new ICloneable member path (N3). There are also three nits.

The new reference-assembly rule makes a ProjectReference DTO that is not auto-property-only (or a record) report TWSG002. I judge that reasonable. It follows the "build error, not silent loss" requirement, and the TWSG002 text names the fix (`ProduceReferenceAssembly=false`). It is documented consistently in cloning.md, the migration guide, the release notes and analyzers.md. test-app-contracts opts in with a comment.

The M10 suppressions are method-scoped, each has a justification and a follow-up note, and the IL ids are listed in `WarningsAsErrors`. One justification is factually off (N6).

## Prior findings
| ID | Status | Evidence |
|----|--------|----------|
| M1 | fixed | p1/p4/p5 re-run: `Wrapper<Item>`, `HistState : ListState<HistState>`, tuple, `KeyValuePair`, and `Nullable<struct>` all clone without `MissingFieldException`. Holders are generic per definition (planner:1395). |
| M2 | fixed | p3 (`IList`, `IReadOnlyList`, `ISet`, `ICollection`, `IReadOnlyCollection`, `IDictionary`, `IReadOnlyDictionary`) builds. Cases are gated by `HasImplicitConversion` (planner:1034). `TryElementType` checks the interface itself. |
| M3 | fixed | p5: `IReadOnlyCollection<-ImmutableList` and `IReadOnlyDictionary<-ImmutableDictionary` clone. Materializing fallbacks exist (planner:931, 969). |
| M4 | fixed | p1: `SortedDictionary` clones 2 of 2 entries and `StringBuilder` gives 'hello'. New probe: a ProjectReference class with a hidden private field and a public method is TWSG002. An implementation-assembly class with a private field is TWSG002. A record with only auto-properties builds. |
| M5 | fixed for source subclasses | p1: `Slicing: clone type=Circle`. Two gaps remain, one in the declaring assembly (N2) and one for non-nameable or generic source subclasses (N1). |
| M6 | fixed | p2 `Money(decimal)`/`Money(string)` builds. New probe: a constructor with `decimal`, `long.MinValue`, enum, `char`, `float`, escaped string, `int? = null` and `NaN` defaults builds. |
| M7 | fixed | p2 `RequiredHolder : RequiredBase` builds. `RequiredInitializer` walks the hierarchy and includes fields (planner:1292). |
| M8 | fixed | p1: `ImmutableStack order: orig top=2 clone top=2`. |
| M9 | fixed | p2 (`ImplicitUsings` disabled) builds. No `.Select(` is emitted. |
| M10 | fixed | `dotnet build timewarp-state.csproj --no-incremental` reports no IL2xxx/IL3xxx. `IsAotCompatible=true` and the IL ids are in `WarningsAsErrors`. The suppressions are method-scoped with justifications. They are listed in Results and the release notes. One justification is inaccurate (N6). |
| M11 | fixed | `state-clone-shape-tests.cs` and `generated-clone-shape-tests.cs` are added. The 51 generator tests pass, and timewarp-state-tests passes 94 with 1 skipped. |
| M12 | fixed (minimum) | The unused syntax provider is gone. The closed-generic scan binds only matching names. Whole-compilation planning is documented in the generator's Design. |
| M13 | fixed | Probes report one TWSG002 at the source member line (for example `Program.cs(7,51)`), use `ToDisplayString()` names, and include "reached from" when a non-root type fails. |
| M14 | fixed | Names carry an FNV-1a hash and a counter fallback (planner:2641). The `NameCollision` shape test is present. |

## Issues

### Issue N1 — Severity: bug
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1819 (also `FindImplementations` at 1808)
- Description:
  - `FindDerived` and `FindImplementations` silently drop subtypes in the compilation that generated code cannot name: `CanName` is false for private or protected nested types, and generic definitions are skipped too. The dispatch then throws `InvalidOperationException` at run time. Its message, "the clone source generator did not see that type in this compilation", is false, because the type was seen and filtered out.
  - Verified with a probe: `public class Base { public int V {get;set;} }` and a `[GenerateClone] class H30 { public Base? B {get;set;} private sealed class PrivBase : Base {...} }`. Setting `B = new PrivBase()` builds cleanly, then throws `Cannot clone H30+PrivBase as Base` on Clone.
  - The interface variant only reports TWSG002 when the private type is the *only* implementation. With one public implementation and one private one, the private one is a runtime throw.
  - A generic subclass `Gen<T> : Base` is also a runtime throw. The docs list it as a limitation, but the generator knows it exists.
  - This contradicts "unsupported must be build errors". cloning.md:46 says the throw "only happens for a subclass declared in another assembly (or a generic subclass)", which is inaccurate for the private-nested case.
- Suggestion: When a derived type or implementation exists in `SourceTypes` but is filtered out (cannot be named, or is an open generic definition), fail the dispatching slot with TWSG002 at that subtype's or the member's location, unless the subtype implements `ICloneable`. As a minimum, correct the runtime message and cloning.md.
- Status: open

### Issue N2 — Severity: bug
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1819 (`FindDerived`) and 1053 (`ClassifyInterface`)
- Description:
  - Subtypes are searched only in the current compilation's source types. A polymorphic DTO hierarchy declared in a contracts project, which is the usual Blazor client/server split (for example `[JsonDerivedType]` contracts), therefore fails in two ways, and the old `DeepCloner` handled both:
    - **Non-abstract base: runtime throw.** Probe: contracts built with `ProduceReferenceAssembly=false`, with `class Animal` and `class Dog : Animal`, and a state member `Animal? A` holding a `Dog`. The build succeeds, then clone throws `Cannot clone ContractsImpl.Dog as ContractsImpl.Animal: the clone source generator did not see that type in this compilation. Implement ICloneable on it.`
    - **Abstract base: build error.** Probe: `abstract class Shape` with `Circle : Shape` gives TWSG002 "interface or abstract type 'ContractsImpl.Shape' has no cloneable implementation in this compilation". This happens even with `ProduceReferenceAssembly=false`, and the TWSG002 guidance offers no fix besides `ICloneable`.
  - The derived types are visible to the generator as metadata symbols in the base's own assembly, and they would pass the new metadata visibility checks.
- Suggestion: For a base or interface declared in a referenced assembly, also enumerate the nameable types of that declaring assembly (and of referenced assemblies that reference it, if cheap) as candidate subtypes, running each through the existing metadata checks. Otherwise, report a diagnostic rather than emitting a runtime throw when a metadata base has visible subtypes. Update cloning.md "Polymorphic members" to match.
- Status: open

### Issue N3 — Severity: bug
- File: source/timewarp-state-source-generator/state-clone-planner.cs:207 (with `ClassifyCloneable` at 475)
- Description:
  - `Build` wraps every non-sealed `Clone` slot for runtime dispatch, including slots that `ClassifyCloneable` produced. A subclass that inherits `ICloneable` gets its own `ClassifyCloneable` slot, which casts the result to the *subclass* (`({name})cloned`).
  - When the base's `Clone()` returns the base type, which is common with `new Base { ... }` instead of `MemberwiseClone`, the clone throws `InvalidCastException`.
  - Verified with a probe: `class Cl : ICloneable { object Clone() => new Cl{...}; }`, `class ClD : Cl`, and `[GenerateClone] class H9 { Cl? C }` holding a `ClD`. The build succeeds, then clone throws `Unable to cast object of type 'Cl' to type 'ClD'` in `Clone_ClD_..._Exact`.
  - Before the wrap (and in the round-1 code), the member slot called `ICloneable.Clone()` and cast to the declared type `Cl`, which does not throw.
- Suggestion: Do not wrap `ICloneable` slots with `WrapForRuntimeType`, because `ICloneable.Clone()` is already the user's virtual dispatch. Alternatively, cast the `ICloneable` result to the member's declared type rather than to the runtime subtype. Add a shape or runtime test.
- Status: open

### Issue N4 — Severity: nit
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1696 (`CanName`)
- Description:
  - `CanName` does not check `INamedTypeSymbol.IsFileLocal`, so a `file` type is treated as nameable.
  - Verified: `[GenerateClone] file class H12 { public int V {get;set;} }` produces CS0400 ("'H12' could not be found in the global namespace") five times in `TimeWarpStateClones.g.cs` instead of TWSG002. A `file sealed class X : State<X>` would hit the same problem.
  - This is rare, but it is a generated-code compile error for valid user code.
- Suggestion: Treat `IsFileLocal` types as not nameable in `CanName`, so they report TWSG002 "not accessible to generated code".
- Status: open

### Issue N5 — Severity: nit
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1661, and the Design text at lines 13–15 and 1545–1548
- Description:
  - **Wrong advice for framework types.** The reference-assembly TWSG002 always says "build '{assembly}' with ProduceReferenceAssembly=false", including for framework reference packs. Verified: an `Exception?` member reports "...comes from reference assembly 'System.Runtime'... (build 'System.Runtime' with ProduceReferenceAssembly=false ...)", which is not actionable.
  - **Design comments overstate the rule.** They say reference-assembly "non-collection classes and managed structs are TWSG002". The code (and cloning.md, correctly) accepts auto-property-only types and records. For example, a ProjectReference struct with a hidden private `List<int>` field and only auto-properties builds and is copied by value.
- Suggestion: Emit the `ProduceReferenceAssembly` hint only when the assembly is a project or package reference, not a framework pack (for example, when the assembly name does not start with `System.`/`Microsoft.`, or when it is not from the targeting pack). Align the two Design comments with the auto-property rule.
- Status: open

### Issue N6 — Severity: nit
- File: source/timewarp-state/extensions/service-collection-extensions.add-timewarp-state.cs:74
- Description:
  - The comment and the IL2026/IL2072 justifications say console hosts "do not trim application assemblies by default (TrimMode=partial)". That holds for Blazor WebAssembly. For console `PublishTrimmed` (since .NET 7) and `PublishAot`, the default is `TrimMode=full`, which does trim the application assembly.
  - The scan works there in practice: Results report `clone-ok` for trimmed and Native AOT console hosts, because the generated `StateCloneRegistry` module initializer statically references each state type and the constructor the clone calls. So the justification states the wrong reason.
- Suggestion: Reword the justification. State types are rooted by the generated clone registration. A state whose DI constructor differs from the one the clone calls may need to be rooted under `TrimMode=full`.
- Status: open
