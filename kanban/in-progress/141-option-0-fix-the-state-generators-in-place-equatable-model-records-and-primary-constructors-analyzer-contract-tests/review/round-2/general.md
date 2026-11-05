# Round 2 — general
**Date:** 2026-11-06
**Scope reviewed:** fix delta `6ab9405d..8c4f08d9` (fix pass 1 for round-1 M1, M2, M3): `action-set-shape.cs`,
`action-set-constructor-parser.cs`, `action-set-method-generator.cs`, `action-catalog-source-generator.cs`, the new
and changed generator tests, `analyzers.md`, `action-catalog.md`, the analyzer readme and the beta.17 release notes.

## Summary

Fix pass 1 closes all three round-1 bugs.
- **M1:** `ActionSetShape.WasServedBefore` matches the old predicate. The Action and the state must both be
  non-record classes. The ActionSet is already limited to `ClassDeclarationSyntax` by `IsCandidateSyntax`. TWSG003
  is now reported only for those shapes. Every other failure returns null.
- **M2:** a plain class Action takes its first explicit constructor before the primary one.
- **M3:** the catalog keeps a hand-written method only when `IsCallableByCatalog` passes. That means the same
  parameter types, `Task` return, a trailing `externalCancellationToken`, matching defaults, an instance method,
  and accessible.

The docs and release notes match the code. Re-verification:
- **Generator suite:** `dotnet test tests/timewarp-state-source-generator-tests`, 150 passed, 0 failed.
- **Test app:** `dotnet build tests/test-app/test-app-server` succeeded with 0 errors. That build includes the
  hand-written `ApplicationState.FiveSecondTask` beside its record Action.
- **Scratch consumer** (`/tmp/rev141-r2`, referencing this branch's projects): the four round-1 repros now compile.
  They are a record Action in a non-partial state, a record Action named like a state property, a class Action with
  primary and explicit constructors whose existing caller is `Set("x")`, and a cataloged record Action with
  `Wait(CancellationToken = default)`.

One probe of the M3 rule still fails, but only for a contrived shape (Issue 1).

## Issues

### Issue 1 — Severity: nit
- File: source/timewarp-state-source-generator/action-set-shape.cs:113
- Description: `IsCallableByCatalog` does not check `RefKind`. Here is the case:
  - a `[CatalogAction]` record `Action(int Amount)`;
  - a hand-written `Task Bump(ref int amount, CancellationToken? externalCancellationToken = null)`.

  That method passes the check. The executor then passes the argument by value, so the build fails with
  `CS1620: Argument 1 must be passed with the 'ref' keyword` (confirmed in the scratch project). On master, records
  were not cataloged, so this compiled.
  The shape is contrived. The method must copy the generated signature exactly, including the
  `externalCancellationToken` name. It must then add `ref` or `out`, and it cannot be `async` (CS1988).
- Suggestion: also require `method.Parameters.All(p => p.RefKind is RefKind.None or RefKind.In)` in
  `IsCallableByCatalog`.
- Status: open

## Re-verified prior findings

- **M1 — fixed.** `action-set-method-generator.cs:100` returns null when the shape was not served before.
  Seven tests in `Should_Skip_Silently_Shapes_Not_Served_Before` cover it. The scratch repros compile.
- **M2 — fixed.** `SelectConstructor` has `explicitFirst` for a non-record class. The
  `Given_Class_With_Primary_And_Explicit_Constructors_Keeps_The_Explicit_Signature` test expects `Set(string name, …)`.
  The scratch caller `Set("x")` compiles.
- **M3 — fixed.** `action-catalog-source-generator.cs:94` skips the entry when the method is not `UserMethodCallable`.
  There are two new catalog tests, one for a skip and one for a call. The scratch repro compiles.
