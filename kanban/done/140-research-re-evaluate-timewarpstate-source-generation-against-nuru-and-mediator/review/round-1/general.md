# Round 1 — general
**Date:** 2026-11-06
**Scope reviewed:** `git diff master...HEAD`: `report/analysis.md`, `report/spike/action-set-generator-spike.cs`, `report/spike/action-set-generator-spike.output.txt`, `task.md`.

## Summary

The report covers every requirement in the task and in the implementer's restatement:

- inventories of State, Mediator and Nuru with citations
- a comparison on each requested dimension
- options 0-5, with developer-written and generated code, costs, and what each unblocks
- no decision taken and no tasks split off

The restatement matches the task, so there is no misread.

I checked the claims against the repositories:

- **Spike:** I re-ran it from a temp directory against a fresh Debug build of the generator. Its stdout matches `action-set-generator-spike.output.txt` exactly; the only differences are runfile IL2026/IL2072 build warnings.
- **Sibling citations:** I checked 21 Mediator, Nuru and 464 citations with a read-only helper. None is false. A few line ranges and one file count are off (M2).
- **State citations:** I checked them directly, at these lines:
  - `action-set-method-generator.cs:29-35,46-48,78-92,107-132,158-179`
  - `persistence-state-source-generator.cs:44-52`
  - `state-clone-source-generator.cs:11,37-39`
  - `check-complex-parameter-changed.cs:24-35`
  - `persistence-service.cs:83`
  - `state-snapshot-recorder.cs:36-37,120-126`
  - the test-app `IncrementCountActionSet`
  - the test driver `:36-40`

  They hold.

I found one factual error in a cost statement Steve would use to decide (M1).

## Issues

### Issue 1 — Severity: suggestion
- File: kanban/in-progress/140-research-re-evaluate-timewarpstate-source-generation-against-nuru-and-mediator/report/analysis.md:208
- Description: Option 0's cost says "`[CatalogAction]` on a record action starts producing executors". The Option 0 change list (`:174-183`) does not deliver that:
  - The catalog generator's `ForAttributeWithMetadataName` predicate is `node is ClassDeclarationSyntax` (`source/timewarp-state-source-generator/action-catalog-source-generator.cs:33`). A `record Action(...)` is a `RecordDeclarationSyntax`, so it is filtered out before `GetEntry` runs.
  - The shared `ActionSetConstructorParser.GetParameters` takes a `ClassDeclarationSyntax` and reads only an explicit `ConstructorDeclarationSyntax` (`action-set-constructor-parser.cs:27-38`).
  - Changing only the ActionSet generator, as Option 0 lists, therefore leaves record actions invisible to the catalog.

  The §4.1 catalog row also does not list this record gap under what the generator cannot see.
- Suggestion:
  - Add "catalog generator: accept `TypeDeclarationSyntax` (records and primary constructors) and share the new parameter reader" to Option 0's "What changes".
  - Note the gap in the §4.1 catalog row.
  - Optionally also note in §4.1 that the parser takes the *first* descendant constructor (`FirstOrDefault`), not a chosen public one. An action with several constructors gets the method for whichever is declared first.
- Status: open

### Issue 2 — Severity: nit
- File: kanban/in-progress/140-research-re-evaluate-timewarpstate-source-generation-against-nuru-and-mediator/report/analysis.md:93
- Description: Some citations are off. The claims they support are correct.
  - `MG/candidate-facts.cs:18-60` (`:93`): `TypeFact` is at 26-30 and `BehaviorFact` at 53-65. The fact records span 19-65.
  - `MG/mediator-model.cs:208-275` (`:99`): the file has 89 lines. `LocationInfo` is at 21-53 and `DiagnosticInfo` at 55-89.
  - `2026-10-13-15.0-breaking-redesign-review.md:285-300` (`:105`, `:423`): F9 is at 287-301. Lines 275-285 are F8.
  - "17 files and 3,866 LOC" (`:106`): `MG/` has 16 `*.cs` files. The 3,866 LOC is exact.
  - "5,037 LOC, 31 files" (`:41`): `SG/` has 29 `*.cs` files. The 5,037 LOC is exact.
- Suggestion: correct the ranges and counts.
- Status: open

### Issue 3 — Severity: nit
- File: kanban/in-progress/140-research-re-evaluate-timewarpstate-source-generation-against-nuru-and-mediator/report/analysis.md:27
- Description: The executive summary says "Any nested `*ActionSet` class in any class produces broken code". That is broader than what the spike and §4.1 show:
  - Inside a partial `State` class, the generated code is correct.
  - The broken case is a `*ActionSet` that contains a nested class `Action` and sits in a class that is not a partial state.
  - task.md Results words this correctly ("outside a state").
- Suggestion: change to "a nested `*ActionSet` with an `Action` class inside any non-state class produces broken code".
- Status: open
