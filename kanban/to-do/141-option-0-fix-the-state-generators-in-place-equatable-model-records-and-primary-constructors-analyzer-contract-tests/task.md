# Option 0: fix the State generators in place

## Description

Source: task 140 `report/analysis.md` (options as code, costs, what each unblocks). Read it first. Option 0 from Steve's 2026-11-06 decision (0 → 1 → 3b → 5 → 4; 2 any time).

Fix the four generators where they are. No new graph yet (that is Option 1).

## Requirements

- ActionSet method generator: equatable record model with `WithTrackingName` on every step; semantic discovery (parent derives from `State<T>`, Action implements `IAction` or is a nested `Action` class or record); read primary constructors and positional records; `required`/`init` properties as parameters; drop SG002; report a TWS diagnostic instead of emitting broken code; direct tests.
- Catalog generator: widen the predicate to `TypeDeclarationSyntax`; the shared constructor parser reads a primary-constructor or positional-record parameter list before an explicit constructor.
- Persistence generator: match `[PersistentStateAttribute]` too, or use `ForAttributeWithMetadataName`.
- Every generator: an incrementality test in the Mediator style.
- **Analyzer contract (gap found after the 140 review):** add a table to the kitchen mapping every assumption a generator makes about its input to the TWS rule that guards it. For each unguarded assumption decide analyzer or generator fix. Today no TWS rule covers action constructors, so a primary-constructor Action reaches the generator and emits CS7036. Where the right fix is a rule, add it with release tracking; where it is the generator, fix the generator. Record the table in a Design region of the generator it describes.
- **Ramani (gap found after the 140 review):** inventory `timewarp-taratibu/source/timewarp-ramani-generators/` (locators, interpreter, emitters) the way 140 did Nuru and Mediator, and record in the kitchen anything it does that should change this task's shape.
- Not breaking: code that failed before now compiles; nothing that compiled changes meaning.
- Version bump per the repo's release rule.

## Checklist

- [ ] Analyzer-contract table and Ramani inventory in the kitchen
- [ ] ActionSet generator fixed with tests
- [ ] Catalog and persistence generators fixed
- [ ] Incrementality tests on every generator
- [ ] Build 0/0, tests green, release tracking updated

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06)

## Notes

Steve 2026-11-06: all of 0, 1, 2, 3b, 4, 5; cockpit set the order. First of the family.
