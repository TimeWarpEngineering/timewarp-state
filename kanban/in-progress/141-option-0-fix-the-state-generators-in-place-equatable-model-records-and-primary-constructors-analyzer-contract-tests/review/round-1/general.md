# Round 1 — general
**Date:** 2026-11-06
**Scope reviewed:** branch vs merge-base with master (see review-framework.md), all four generators, the shared shape/parser, new tests, docs and release notes.

## Summary

The change does what the task asks for code that failed before. The equatable models, named steps, semantic
discovery, the TWSG003 rule, the Design-region contract tables and the incrementality tests are all present. The
generator suite passes (140 tests). The dominant risk is the hard "not breaking" requirement. Three shapes that
compiled on master stop compiling on this branch. In each case the old generator either ignored the input (records,
structs) or picked a different constructor. Each was confirmed with a scratch project in `/tmp/rev141-scratch` that
references this branch's `timewarp-state` and `timewarp-state-source-generator` projects. The old-behavior half
comes from reading the master generator source: its predicate matched only `ClassDeclarationSyntax` for the Action,
and it took the first `ConstructorDeclarationSyntax`.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state-source-generator/action-set-shape.cs:63 (with action-set-method-generator.cs:93)
- Description: `FindFailure` runs for every action set, including shapes the old generator never served. These are a
  record or struct Action, and an ActionSet inside a record or struct state. On master those shapes produced no
  generated code, so they compiled. On this branch any failure becomes TWSG003, which is an Error, so the build
  breaks. Confirmed:
  - a `record Action` in a non-partial `State<T>` gives `error TWSG003 … 'Scratch.PlainState' is not partial`;
  - a `record Action(int Value)` in a partial state that also has a property `Tok` gives `error TWSG003 … already has a property named 'Tok'`.
  The same holds for an abstract or private record, a record parameter named `externalCancellationToken` (the test
  `Given_Parameter_Named_Like_The_Generated_Token` pins this break on a record), and a private parameter type on a
  record. The task says "nothing that compiled changes meaning", and the release notes and `analyzers.md` say "each
  case used to emit generated code that failed to compile". Neither claim holds for these shapes.
- Suggestion: report TWSG003 only for shapes that were served before: a plain non-record class Action whose state is
  a class. That is the set where the failure really replaces broken generated code. For a newly served shape, a
  failure should skip silently or report a Warning or Info, so the build behaves as before. Update the TWSG003 tests
  that use records, and correct the wording in the release notes and `analyzers.md`.
- Status: open

### Issue 2 — Severity: bug
- File: source/timewarp-state-source-generator/action-set-constructor-parser.cs:83 (test pinning it: tests/timewarp-state-source-generator-tests/action-set-method-generator-tests.cs:153)
- Description: Take a non-record class Action with both a primary constructor and an explicit constructor, such as
  `class Action(string name, int count) { public Action(string name) : this(name, 0) {} }`. On master this generated
  `Set(string name)`, because the old parser took the first `ConstructorDeclarationSyntax`, and that compiled. On
  this branch the primary constructor wins and the method becomes `Set(string name, int count)`. An existing call
  `state.Set("x")` now fails with CS7036 (confirmed in the scratch project). The catalog shares the parser, so the
  catalog entry's parameters change too. The test `Given_Primary_And_Explicit_Constructors_Prefers_Primary` asserts
  the breaking signature. This contradicts restatement criterion 7 ("no signature change for any action shape that
  already produced a compiling method").
- Suggestion: for a plain class Action, which was served before, keep the old choice: the first explicit instance
  constructor when one exists, and the primary constructor only when there is no explicit one. Records and structs
  keep primary first. This still meets the requirement to read primary constructors, because every case where only
  a primary constructor exists used to be CS7036. Flip the test so it pins the old signature.
- Status: open

### Issue 3 — Severity: bug
- File: source/timewarp-state-source-generator/action-catalog-source-generator.cs:88
- Description: `GetEntry` keeps an entry when `analysis.MethodDeclaredByUser` is true. For a record or struct
  Action, `IsDeclaredByUser` is true for *any* same-name method, whatever its signature. The executor then calls
  `state.Method(<parser parameters>, externalCancellationToken: cancellationToken)` on the hand-written method. On
  master, records were never cataloged (the predicate was `ClassDeclarationSyntax`), so `[CatalogAction]` on a
  record with a hand-written method compiled. Now, for example with `Task Wait(CancellationToken cancellationToken = default)`,
  the generated catalog fails:
  `error CS1739: The best overload for 'Wait' does not have a parameter named 'externalCancellationToken'` (confirmed).
  A hand-written method with different parameters or a `ValueTask` return breaks the same way.
- Suggestion: catalog a user-declared method only when its signature is the generated one, meaning the same check
  `IsDeclaredByUser` already does for class Actions. Otherwise skip the entry, as master effectively did. Add a
  catalog test for a record Action with a hand-written method.
- Status: open
