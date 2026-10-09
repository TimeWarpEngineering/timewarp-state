# Round 3 — merged findings
**Date:** 2026-10-10
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 15 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 4 | 0 |

## Resolved prior

M1–M14: fixed, no regression (p1–p5 re-run). M15–M20: fixed in 858de934 and re-verified by probe. M15 has a gap for generic subclasses of generic bases, tracked as M21 (now fixed).

## Issues

### M21 — Severity: bug — Status: fixed
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
- Source: general (round-3 R1)
- Disposition notes:
  - Fixed by constructing the closed subtype (the preferred option). `FindSubtypes`/`FindHiddenSubtype` now go through `Subtypes`, which keeps the exact-match path and adds `CloseOver` for generic definitions. `CloseOver` takes each base-chain type (and, for an interface target, each interface) whose `OriginalDefinition` is the target's definition, unifies it with the target to bind the candidate's type parameters, constructs the candidate (`GD<T>` for `GB<int>` gives `GD<int>`, `Ok<T>` for `Result<List<string>>` gives `Ok<List<string>>`), checks the constraints (class/struct/unmanaged/new() and substituted constraint types through identity, reference or boxing conversions), and re-checks `IsConcreteSubtype` on the constructed type. That type then goes through the normal case and clone/metadata checks.
  - A definition that unifies but leaves a type parameter unbound, or whose constraints fail, is returned as the definition, which `CanCase` rejects, so the M15 rule reports TWSG002. A definition whose shape cannot unify (`AList<T> : AB<List<T>>` for `AB<int>`) is not a subtype and is ignored. The TWSG002 detail now says "generic with type parameters the member type does not determine".
  - Tests: shape tests `GenericSubclassOfGenericClass`, `GenericSubclassOfAbstractGenericClass` (includes `abstract record Result<T>` with `Ok<T>`/`Err<T>` and a non-unifiable `AList<T>`), `GenericImplementationOfGenericInterface` (class and struct) compile clean and emit `case` arms for the closed types. `GenericSubclassWithUndeterminedParameter`, `GenericSubclassWithFailingConstraint`, `GenericImplementationWithUndeterminedParameter` are TWSG002 at source. Runtime test `Keep_Runtime_Type_Of_Generic_Subtypes_Of_Generic_Members` clones all three hierarchies and the `Result<T>` records.
  - Probe `/tmp/rv097-r3/app/Gen.cs`: GENBASE `genbase: GD`1 V=1 W=2 same=False`, GENIFACE `geniface: GR`1`, GENABS `genabs: AD`1`, new GENRESULT `genresult: Ok`1 sameList=False Err { Message = bad }`, new GENUNBOUND and GENCONSTRAINT give TWSG002. The other r3 probes and p1–p5 are unchanged.
  - cloning.md describes the closing rule and which generic subtypes are TWSG002.

## Duplicates / conflicts

- Round-3 R1 maps to M21.
