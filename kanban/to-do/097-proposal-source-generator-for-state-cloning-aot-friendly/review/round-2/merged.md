# Round 2 — merged findings
**Date:** 2026-10-10
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 3 | 11 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 3 | 1 | 0 |

## Resolved prior (round 1)

M1–M14: fixed in ab291e16; re-verified by the round-2 general reviewer (probes p1–p5 rebuilt and re-run). M5 is fixed for same-project subclasses; the remaining gaps are M15 and M16. M12 is fixed at the minimum (unused syntax provider removed).

## Issues

### M15 — Severity: bug — Status: open
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1819 (also `FindImplementations` at 1808)
- Description:
  - `FindDerived` and `FindImplementations` silently drop subtypes in the compilation that generated code cannot name: `CanName` is false for private or protected nested types, and generic definitions are skipped too. The dispatch then throws `InvalidOperationException` at run time. Its message, "the clone source generator did not see that type in this compilation", is false, because the type was seen and filtered out.
  - Verified with a probe: `public class Base { public int V {get;set;} }` and a `[GenerateClone] class H30 { public Base? B {get;set;} private sealed class PrivBase : Base {...} }`. Setting `B = new PrivBase()` builds cleanly, then throws `Cannot clone H30+PrivBase as Base` on Clone.
  - The interface variant only reports TWSG002 when the private type is the *only* implementation. With one public implementation and one private one, the private one is a runtime throw.
  - A generic subclass `Gen<T> : Base` is also a runtime throw. The docs list it as a limitation, but the generator knows it exists.
  - This contradicts "unsupported must be build errors". cloning.md:46 says the throw "only happens for a subclass declared in another assembly (or a generic subclass)", which is inaccurate for the private-nested case.
- Suggestion: When a derived type or implementation exists in `SourceTypes` but is filtered out (cannot be named, or is an open generic definition), fail the dispatching slot with TWSG002 at that subtype's or the member's location, unless the subtype implements `ICloneable`. As a minimum, correct the runtime message and cloning.md.
- Source: general (round-2 N1)
- Disposition notes:

### M16 — Severity: bug — Status: open
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1819 (`FindDerived`) and 1053 (`ClassifyInterface`)
- Description:
  - Subtypes are searched only in the current compilation's source types. A polymorphic DTO hierarchy declared in a contracts project, which is the usual Blazor client/server split (for example `[JsonDerivedType]` contracts), therefore fails in two ways, and the old `DeepCloner` handled both:
    - **Non-abstract base: runtime throw.** Probe: contracts built with `ProduceReferenceAssembly=false`, with `class Animal` and `class Dog : Animal`, and a state member `Animal? A` holding a `Dog`. The build succeeds, then clone throws `Cannot clone ContractsImpl.Dog as ContractsImpl.Animal: the clone source generator did not see that type in this compilation. Implement ICloneable on it.`
    - **Abstract base: build error.** Probe: `abstract class Shape` with `Circle : Shape` gives TWSG002 "interface or abstract type 'ContractsImpl.Shape' has no cloneable implementation in this compilation". This happens even with `ProduceReferenceAssembly=false`, and the TWSG002 guidance offers no fix besides `ICloneable`.
  - The derived types are visible to the generator as metadata symbols in the base's own assembly, and they would pass the new metadata visibility checks.
- Suggestion: For a base or interface declared in a referenced assembly, also enumerate the nameable types of that declaring assembly (and of referenced assemblies that reference it, if cheap) as candidate subtypes, running each through the existing metadata checks. Otherwise, report a diagnostic rather than emitting a runtime throw when a metadata base has visible subtypes. Update cloning.md "Polymorphic members" to match.
- Source: general (round-2 N2)
- Disposition notes:

### M17 — Severity: bug — Status: open
- File: source/timewarp-state-source-generator/state-clone-planner.cs:207 (with `ClassifyCloneable` at 475)
- Description:
  - `Build` wraps every non-sealed `Clone` slot for runtime dispatch, including slots that `ClassifyCloneable` produced. A subclass that inherits `ICloneable` gets its own `ClassifyCloneable` slot, which casts the result to the *subclass* (`({name})cloned`).
  - When the base's `Clone()` returns the base type, which is common with `new Base { ... }` instead of `MemberwiseClone`, the clone throws `InvalidCastException`.
  - Verified with a probe: `class Cl : ICloneable { object Clone() => new Cl{...}; }`, `class ClD : Cl`, and `[GenerateClone] class H9 { Cl? C }` holding a `ClD`. The build succeeds, then clone throws `Unable to cast object of type 'Cl' to type 'ClD'` in `Clone_ClD_..._Exact`.
  - Before the wrap (and in the round-1 code), the member slot called `ICloneable.Clone()` and cast to the declared type `Cl`, which does not throw.
- Suggestion: Do not wrap `ICloneable` slots with `WrapForRuntimeType`, because `ICloneable.Clone()` is already the user's virtual dispatch. Alternatively, cast the `ICloneable` result to the member's declared type rather than to the runtime subtype. Add a shape or runtime test.
- Source: general (round-2 N3)
- Disposition notes:

### M18 — Severity: nit — Status: open
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1696 (`CanName`)
- Description:
  - `CanName` does not check `INamedTypeSymbol.IsFileLocal`, so a `file` type is treated as nameable.
  - Verified: `[GenerateClone] file class H12 { public int V {get;set;} }` produces CS0400 ("'H12' could not be found in the global namespace") five times in `TimeWarpStateClones.g.cs` instead of TWSG002. A `file sealed class X : State<X>` would hit the same problem.
  - This is rare, but it is a generated-code compile error for valid user code.
- Suggestion: Treat `IsFileLocal` types as not nameable in `CanName`, so they report TWSG002 "not accessible to generated code".
- Source: general (round-2 N4)
- Disposition notes:

### M19 — Severity: nit — Status: open
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1661, and the Design text at lines 13–15 and 1545–1548
- Description:
  - **Wrong advice for framework types.** The reference-assembly TWSG002 always says "build '{assembly}' with ProduceReferenceAssembly=false", including for framework reference packs. Verified: an `Exception?` member reports "...comes from reference assembly 'System.Runtime'... (build 'System.Runtime' with ProduceReferenceAssembly=false ...)", which is not actionable.
  - **Design comments overstate the rule.** They say reference-assembly "non-collection classes and managed structs are TWSG002". The code (and cloning.md, correctly) accepts auto-property-only types and records. For example, a ProjectReference struct with a hidden private `List<int>` field and only auto-properties builds and is copied by value.
- Suggestion: Emit the `ProduceReferenceAssembly` hint only when the assembly is a project or package reference, not a framework pack (for example, when the assembly name does not start with `System.`/`Microsoft.`, or when it is not from the targeting pack). Align the two Design comments with the auto-property rule.
- Source: general (round-2 N5)
- Disposition notes:

### M20 — Severity: nit — Status: open
- File: source/timewarp-state/extensions/service-collection-extensions.add-timewarp-state.cs:74
- Description:
  - The comment and the IL2026/IL2072 justifications say console hosts "do not trim application assemblies by default (TrimMode=partial)". That holds for Blazor WebAssembly. For console `PublishTrimmed` (since .NET 7) and `PublishAot`, the default is `TrimMode=full`, which does trim the application assembly.
  - The scan works there in practice: Results report `clone-ok` for trimmed and Native AOT console hosts, because the generated `StateCloneRegistry` module initializer statically references each state type and the constructor the clone calls. So the justification states the wrong reason.
- Suggestion: Reword the justification. State types are rooted by the generated clone registration. A state whose DI constructor differs from the one the clone calls may need to be rooted under `TrimMode=full`.
- Source: general (round-2 N6)
- Disposition notes:

## Duplicates / conflicts

- Round-2 N1–N6 map to M15–M20.
- Orchestrator note: the reviewer saw `sample-04-server` restore fail (NU1102 TimeWarp.State.Telemetry 12.0.0-beta.11). That is expected for a version bump: samples consume the packed local feed at TimeWarpStateVersion. Not a finding.
