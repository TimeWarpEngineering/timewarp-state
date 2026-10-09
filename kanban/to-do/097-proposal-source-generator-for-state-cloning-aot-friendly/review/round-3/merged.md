# Round 3 — merged findings
**Date:** 2026-10-10
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 14 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 4 | 0 |

## Resolved prior

M1–M14: fixed, no regression (p1–p5 re-run). M15–M20: fixed in 858de934 and re-verified by probe. M15 has a gap for generic subclasses of generic bases, tracked as M21.

## Issues

### M21 — Severity: bug — Status: open
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

## Duplicates / conflicts

- Round-3 R1 maps to M21.
