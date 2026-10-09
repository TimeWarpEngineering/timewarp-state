# Round 3 — general
**Date:** 2026-10-10
**Scope reviewed:** fix delta b3e9e0f5..HEAD plus re-verification of M15–M20 (and an M1–M14 regression check)

## Summary

I confirmed fixes for M15–M20 with probes under `/tmp/rv097-r3`. Round-1 probes p1–p5 rebuild and print the same correct results as in round 2, so M1–M14 have not regressed. `timewarp-state-source-generator-tests` passes 66/66. `timewarp-state-tests` passes 95 with 1 skipped (the Fixie sample).

One new bug: the M15 hidden-subtype rule and the subtype search miss a **generic subclass of a generic base**. `Inherits` and the `AllInterfaces` check compare the candidate's constructed base, such as `GB<T>` from `GD<T> : GB<T>`, with the member's closed type `GB<int>`. These never match, so the generic definition is neither a case nor "hidden". The build succeeds and the clone throws at run time. The runtime message is also wrong for this case: it says the type is in an unreferenced or framework assembly, but the type is in the project.

**M15 strictness.** I judge it acceptable and not a false-positive generator. The rule fires only when a concrete subtype of the member's exact base or interface exists that generated code cannot name. At build time such a value can always reach the member. The old `DeepCloner` would have cloned it, and the new code could only throw. The escape hatch, `ICloneable` on the subtype, is documented in TWSG002 and in cloning.md. The rule does not touch collection interfaces or known BCL collections: `ClassifyCollection` runs before `ClassifyInterface`, and `WrapForRuntimeType` skips the check for known collections. A private nested type implementing `IEnumerable<T>` therefore does not break `IEnumerable<T>`/`IReadOnlyList<T>` state members. The remaining risk is rare: a private or generic subclass of a base used in state that in practice never reaches state. That is a deliberate trade-off.

**M16 scan.** It is correct in these probes:
- A concrete metadata base with a subtype in the implementation assembly clones as `Dog`.
- An abstract base works from both the reference and the implementation assembly.
- A **sibling** assembly that references the contracts assembly is scanned. `Sibling.Cat : ContractsImpl.Animal` clones as `Cat` with its fields. `Sibling.Square : Contracts.Shape` from a reference assembly clones.
- An `internal` sibling subtype is TWSG002 at the member.

The cost is reasonable. Framework targets skip the scan entirely. Per-assembly type lists are cached for the planner run, and the per-call work is a linear walk of the referenced-assembly list plus a filter over the cached candidates.

## Prior findings
| ID | Status | Evidence |
|----|--------|----------|
| M1–M14 | fixed (no regression) | p1–p5 copied to /tmp/rv097-r3/p, rebuilt against HEAD, all build. p1: ImmutableStack top=2, SortedDictionary 2/2, Slicing=Circle, Nullable<struct> sameList=False, KVP sameItem=False, StringBuilder 'hello', ReadOnlyCollection 1. p4: Wrapper<Item>, HistState, Record, RoStruct, InnerState correct. p5: tuple and IReadOnly* from Immutable* clone. p2/p3 build. |
| M15 | fixed (gap, see R1) | PRIVSUB probe (H30 with `private sealed class PrivBase : Base`): TWSG002 at `B` naming `H30.PrivBase`, no runtime throw. The private-nested, protected, file and non-generic-base generic cases are covered by shape tests. A generic subclass of a *generic* base is missed (R1). |
| M16 | fixed | POLY_CONCRETE_IMPL prints `poly concrete: Dog`. POLY_ABSTRACT_REF and POLY_ABSTRACT_IMPL build. New SIB probe: sibling-assembly `Cat`/`Square` clone by runtime type with fields copied. SIBINTERNAL (`internal class Ghost : ContractsImpl.Animal` in the sibling): TWSG002 at the member. |
| M17 | fixed | CLONEABLE_NONSEALED (Cl/ClD) prints `Cl V=101`, with no InvalidCastException. CLONEABLE_IFACE builds. |
| M18 | fixed | FILELOCAL (`file class H12`): TWSG002 "not accessible to generated code", with no CS0400. |
| M19 | fixed | EXCEPTION (`Exception?`): TWSG002 without the ProduceReferenceAssembly hint. The Design comments now describe the auto-property/record rule. |
| M20 | fixed | The comment and IL2026/IL2072 justifications now cite TrimMode=full and rooting through the StateCloneRegistry registration (diff reviewed). |

## Issues

### Issue R1 — Severity: bug
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1862 (`IsConcreteSubtype`), with `Inherits` at 1928
- Description:
  - A generic subclass of a generic base or interface in the same project is not seen by `FindDerived`/`FindImplementations`, and not by the M15 `FindHiddenSubtype` either.
  - `Inherits(GD<T>, GB<int>)` walks `GD<T>.BaseType`, which is `GB<T>` and is never equal to `GB<int>`. The interface check `AllInterfaces.Contains(IR<int>)` fails the same way for `GR<T> : IR<T>`.
  - The member is therefore treated as having no subtype. The build succeeds, and at run time the clone throws `InvalidOperationException`. The message, "...declared in an assembly this project does not reference, or in a framework assembly", is false here.
  - The old `DeepCloner` cloned these values, and the hard rule requires a build error rather than a runtime throw for a subtype the generator saw.
  - This hierarchy is common, for example `abstract record Result<T>` with `record Ok<T>(T Value) : Result<T>`.
  - Verified (build succeeds, run throws) with `/tmp/rv097-r3/app/Gen.cs`:
    - GENBASE: `class GB<T>`, `class GD<T> : GB<T>`, member `GB<int>?` holding `GD<int>` gives `THROW InvalidOperationException: Cannot clone GD`1[[System.Int32...]] as GB<int>: the clone source generator did not see that type at build time ...`
    - GENIFACE: `IR<T>`, `IntR : IR<int>`, `GR<T> : IR<T>`, member `IR<int>?` holding `GR<int>` throws the same way.
    - GENABS: `abstract AB<T>`, `AD<T> : AB<T>`, `AInt : AB<int>`, member `AB<int>?` holding `AD<int>` throws the same way.
  - This is not a regression (pre-round-2 code also missed it), but it is a remaining hole in the M15 "generic subclass is TWSG002" claim made in cloning.md.
- Suggestion:
  - In `IsConcreteSubtype`, when the candidate is a generic definition, also match the target by `OriginalDefinition` along the base chain and in `AllInterfaces`. Then the M15 rule reports TWSG002 for it, since `CanCase` already rejects generic definitions.
  - Optionally, construct the candidate (for example `GD<int>`) when its type parameters map one-to-one onto the target's type arguments, and dispatch to it instead of failing.
  - Add shape tests for a generic subclass of a generic class, of an abstract generic class, and of a generic interface.
- Status: open
